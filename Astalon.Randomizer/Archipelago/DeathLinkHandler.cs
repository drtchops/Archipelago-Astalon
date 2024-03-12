﻿using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using BepInEx;

namespace Astalon.Randomizer.Archipelago;

public class DeathLinkHandler
{
    private static bool _deathLinkEnabled;
    private readonly string _slotName;
    private readonly DeathLinkService _service;
    private readonly Queue<DeathLink> _deathLinks = new();

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
        _deathLinks.Enqueue(deathLink);

        Plugin.Logger.LogDebug(deathLink.Cause.IsNullOrWhiteSpace()
            ? $"Received Death Link from: {deathLink.Source}"
            : deathLink.Cause);
    }

    /// <summary>
    ///     can be called when in a valid state to kill the player, dequeuing and immediately killing the player with a
    ///     message if we have a death link in the queue
    /// </summary>
    public void KillPlayer()
    {
        try
        {
            if (_deathLinks.Count < 1)
            {
                return;
            }

            var deathLink = _deathLinks.Dequeue();
            // text boxes have to be short, investigate using a dialogue box for showing full cause
            var cause = GetDeathLinkCause(deathLink);

            Plugin.Logger.LogMessage(cause);
            Game.ReceiveDeath(cause);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    /// <summary>
    ///     returns message for the player to see when a death link is received without a cause
    /// </summary>
    /// <param name="deathLink">death link object to get relevant info from</param>
    /// <returns></returns>
    private static string GetDeathLinkCause(DeathLink deathLink)
    {
        return $"Death from {deathLink.Source}";
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

            Plugin.Logger.LogMessage("sharing your death...");

            // add the cause here
            var linkToSend = new DeathLink(_slotName);

            _service.SendDeathLink(linkToSend);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }
    }

    public bool IsEnabled()
    {
        return _deathLinkEnabled;
    }
}