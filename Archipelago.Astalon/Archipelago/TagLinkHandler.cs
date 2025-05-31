using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;

namespace Archipelago.Astalon.Archipelago;

/// <summary>
/// A TagLink object that gets sent and received via bounce packets.
/// </summary>
/// <remarks>
/// A TagLink object that gets sent and received via bounce packets.
/// </remarks>
/// <param name="sourcePlayer">Name of the player sending the TagLink</param>
/// <param name="kong">Optional character for the TagLink. Since this is optional it should generally include
/// a name as if this entire text is what will be displayed</param>
public class TagLink(string sourcePlayer, int kong = 5, bool tag = true) : IEquatable<TagLink>
{
    /// <summary>
    /// The Timestamp of the created TagLink object
    /// </summary>
    public DateTime Time { get; internal set; } = DateTime.UtcNow;
    /// <summary>
    /// The name of the player who sent the TagLink
    /// </summary>
    public string Source { get; } = sourcePlayer;
    /// <summary>
    /// The character to switch to for players receiving the TagLink. Can be null
    /// </summary>
    public int Kong { get; } = kong;
    public bool Tag { get; } = tag;

    internal static bool TryParse(Dictionary<string, JToken> data, out TagLink tagLink)
    {
        try
        {
            if (!data.TryGetValue("time", out var timeStampToken) || !data.TryGetValue("source", out var sourceToken))
            {
                tagLink = null;
                return false;
            }

            var kong = 5;
            if (data.TryGetValue("kong", out var kongToken))
            {
                kong = int.Parse(kongToken.ToString());
            }

            tagLink = new TagLink(sourceToken.ToString(), kong)
            {
                Time = UnixTimeConverter.UnixTimeStampToDateTime(timeStampToken.ToObject<double>()),
            };
            return true;
        }
        catch
        {
            tagLink = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public bool Equals(TagLink other)
    {
        return !ReferenceEquals(null, other)
            && (ReferenceEquals(this, other)
                || (Source == other.Source
                    && Time.Date.Equals(other.Time.Date)
                    && Time.Hour == other.Time.Hour
                    && Time.Minute == other.Time.Minute
                    && Time.Second == other.Time.Second));
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals((TagLink)obj)));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Time, Source);
    }

    public static bool operator ==(TagLink lhs, TagLink rhs)
    {
        return lhs?.Equals(rhs) ?? rhs is null;
    }

    public static bool operator !=(TagLink lhs, TagLink rhs)
    {
        return !(lhs == rhs);
    }
}

public static class TagLinkProvider
{
    /// <summary>
    ///     creates and returns a <see cref="TagLinkService"/> for this <paramref name="session"/>.
    /// </summary>
    public static TagLinkService CreateTagLinkService(this ArchipelagoSession session)
    {
        return new(session.Socket, session.ConnectionInfo);
    }
}

public class TagLinkService
{
    private readonly IArchipelagoSocketHelper socket;
    private readonly IConnectionInfoProvider connectionInfoProvider;
    private TagLink lastSendTagLink;

    /// <summary>
    /// Creates <see cref="OnTagLinkReceived"/> event for clients to hook into and decide what to do with the
    /// received <see cref="TagLink"/>
    /// </summary>
    public delegate void TagLinkReceivedHandler(TagLink tagLink);
    /// <summary>
    /// Delegate event that supplies the created <see cref="TagLink"/> whenever one is received from the server
    /// as a bounce packet.
    /// </summary>
    public event TagLinkReceivedHandler OnTagLinkReceived;

    internal TagLinkService(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider)
    {
        this.socket = socket;
        this.connectionInfoProvider = connectionInfoProvider;

        socket.PacketReceived += OnPacketReceived;
    }

    private void OnPacketReceived(ArchipelagoPacketBase packet)
    {
        switch (packet)
        {
            case BouncedPacket bouncedPacket when bouncedPacket.Tags.Contains("TagLink"):
                if (TagLink.TryParse(bouncedPacket.Data, out var tagLink))
                {
                    if (lastSendTagLink != null && lastSendTagLink == tagLink)
                    {
                        return;
                    }

                    OnTagLinkReceived?.Invoke(tagLink);
                }
                break;
        }
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    ///     Formats and sends a Bounce packet using the provided <paramref name="tagLink"/> object.
    /// </summary>
    /// <param name="tagLink">
    ///     <see cref="TagLink"/> object containing the information of the death which occurred.
    ///     Must at least contain the <see cref="TagLink.Source"/>.
    /// </param>
    /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
    ///     The websocket connection is not alive
    /// </exception>
    public void SendTagLink(TagLink tagLink)
    {
        var bouncePacket = new BouncePacket
        {
            Tags = ["TagLink"],
            Data = new Dictionary<string, JToken> {
                    {"time", tagLink.Time.ToUnixTimeStamp()},
                    {"source", tagLink.Source},
                    {"kong", tagLink.Kong},
                    {"tag", tagLink.Tag},
                }
        };

        lastSendTagLink = tagLink;

        _ = socket.SendPacketAsync(bouncePacket);
    }

    /// <summary>
    /// Adds "TagLink" to your <see cref="ArchipelagoSession"/>'s tags and opts you in to receiving
    /// <see cref="OnTagLinkReceived"/> events
    /// </summary>
    public void EnableTagLink()
    {
        if (Array.IndexOf(connectionInfoProvider.Tags, "TagLink") == -1)
        {
            connectionInfoProvider.UpdateConnectionOptions(
                [.. connectionInfoProvider.Tags, .. new[] { "TagLink" }]);
        }
    }

    /// <summary>
    /// Removes the "TagLink" tag from your <see cref="ArchipelagoSession"/> and opts out of further
    /// <see cref="OnTagLinkReceived"/> events
    /// </summary>
    public void DisableTagLink()
    {
        if (Array.IndexOf(connectionInfoProvider.Tags, "TagLink") == -1)
        {
            return;
        }

        connectionInfoProvider.UpdateConnectionOptions(
            connectionInfoProvider.Tags.Where(static t => t != "TagLink").ToArray());
    }
}

public class TagLinkHandler
{
    private static bool _tagLinkEnabled;
    private readonly string _slotName;
    private readonly TagLinkService _service;
    private readonly Queue<TagLink> _tagLinks = new();

    /// <summary>
    ///     instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
    /// </summary>
    /// <param name="tagLinkService">
    ///     The new TagLinkService that our handler will use to send and
    ///     receive death links
    /// </param>
    /// <param name="name">The slot name for the current player</param>
    /// <param name="enableTagLink">Whether we should enable death link or not on startup</param>
    public TagLinkHandler(TagLinkService tagLinkService, string name, bool enableTagLink = false)
    {
        _service = tagLinkService;
        _service.OnTagLinkReceived += TagLinkReceived;
        _slotName = name;
        _tagLinkEnabled = enableTagLink;

        if (_tagLinkEnabled)
        {
            _service.EnableTagLink();
        }
    }

    /// <summary>
    ///     enables/disables death link
    /// </summary>
    public void ToggleTagLink()
    {
        _tagLinkEnabled = !_tagLinkEnabled;

        if (_tagLinkEnabled)
        {
            _service.EnableTagLink();
        }
        else
        {
            _service.DisableTagLink();
        }
    }

    /// <summary>
    ///     what happens when we receive a tagLink
    /// </summary>
    /// <param name="tagLink">Received Death Link object to handle</param>
    private void TagLinkReceived(TagLink tagLink)
    {
        if (tagLink.Source == _slotName)
        {
            return;
        }

        _tagLinks.Enqueue(tagLink);

        Plugin.Logger.LogDebug($"Received Death Link from: {tagLink.Source}");
    }

    /// <summary>
    ///     can be called when in a valid state to tag the player,
    ///     dequeuing and immediately tagging the player if we have a tag link in the queue
    /// </summary>
    public void TagPlayer()
    {
        try
        {
            if (_tagLinks.Count < 1)
            {
                return;
            }

            var tagLink = _tagLinks.Dequeue();
            Game.ReceiveTag(tagLink.Kong);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    /// <summary>
    ///     called to send a tag link to the multiworld
    /// </summary>
    public void SendTagLink(int character)
    {
        try
        {
            if (!_tagLinkEnabled)
            {
                return;
            }

            Plugin.Logger.LogMessage("sharing your tag...");

            // add the cause here
            var linkToSend = new TagLink(_slotName, character);

            _service.SendTagLink(linkToSend);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    public static bool IsEnabled()
    {
        return _tagLinkEnabled;
    }
}
