using System;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

namespace Archipelago.Astalon.Archipelago;

public class DeathLinkHandler
{
    private static bool _deathLinkEnabled;
    private readonly string _slotName;
    private readonly DeathLinkService _service;

    /// <summary>
    ///     instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
    /// </summary>
    /// <param name="deathLinkService">
    ///     The new DeathLinkService that our handler will use to send and
    ///     receive death links
    /// </param>
    /// <param name="name">The slot name for the current player</param>
    /// <param name="enableDeathLink">Whether we should enable death link or not on startup</param>
    public DeathLinkHandler(DeathLinkService deathLinkService, string name, bool enableDeathLink = false)
    {
        _service = deathLinkService;
        _service.OnDeathLinkReceived += DeathLinkReceived;
        _slotName = name;
        _deathLinkEnabled = enableDeathLink;

        if (_deathLinkEnabled)
        {
            _service.EnableDeathLink();
        }
    }

    /// <summary>
    ///     enables/disables death link
    /// </summary>
    public void ToggleDeathLink()
    {
        _deathLinkEnabled = !_deathLinkEnabled;

        if (_deathLinkEnabled)
        {
            _service.EnableDeathLink();
        }
        else
        {
            _service.DisableDeathLink();
        }
    }

    /// <summary>
    ///     what happens when we receive a deathLink
    /// </summary>
    /// <param name="deathLink">Received Death Link object to handle</param>
    private void DeathLinkReceived(DeathLink deathLink)
    {
        var cause = string.IsNullOrWhiteSpace(deathLink.Cause) ? $"{deathLink.Source} died" : deathLink.Cause;
        Game.QueueDeath(cause);
        Plugin.Logger.LogDebug(cause);
    }

    /// <summary>
    ///     called to send a death link to the multiworld
    /// </summary>
    public void SendDeathLink()
    {
        try
        {
            if (!_deathLinkEnabled)
            {
                return;
            }

            Plugin.Logger.LogMessage("Sharing your death...");

            // add the cause here
            var linkToSend = new DeathLink(_slotName);

            _service.SendDeathLink(linkToSend);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    public static bool IsEnabled()
    {
        return _deathLinkEnabled;
    }
}
