﻿using Dissonance;
using GameNetcodeStuff;
using Scoops.misc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scoops.service
{
    public class PhoneNetworkHandler : NetworkBehaviour
    {
        public static PhoneNetworkHandler Instance { get; private set; }

        private Dictionary<string, ulong> phoneNumberDict;
        private Dictionary<string, PhoneBehavior> phoneObjectDict;

        public PlayerPhone localPhone;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (Instance != null)
                {
                    Instance.gameObject.GetComponent<NetworkObject>().Despawn();
                }
            }

            Instance = this;

            phoneNumberDict = new Dictionary<string, ulong>();
            phoneObjectDict = new Dictionary<string, PhoneBehavior>();

            base.OnNetworkSpawn();
        }

        public void CreateNewPhone(ulong phoneId, int wantedNumber)
        {
            CreateNewPhoneNumberServerRpc(phoneId, wantedNumber);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CreateNewPhoneNumberServerRpc(ulong phoneId, int wantedNumber, ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            int phoneNumber = wantedNumber == -1 ? Random.Range(0, 10000) : Mathf.Clamp(wantedNumber, 0, 10000);

            string phoneString = phoneNumber.ToString("D4");
            while (phoneNumberDict.ContainsKey(phoneNumber.ToString("D4")))
            {
                phoneNumber = Mathf.Clamp(phoneNumber + 1, 0, 10000);
                phoneString = phoneNumber.ToString("D4");
            }

            PhoneBehavior phone = GetNetworkObject(phoneId).GetComponent<PhoneBehavior>();
            Plugin.Log.LogInfo($"New phone for object: " + phoneId);

            phone.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            phoneNumberDict.Add(phoneString, phone.NetworkObjectId);
            phoneObjectDict.Add(phoneString, phone);

            phone.SetNewPhoneNumberClientRpc(phoneString);
        }

        public void DeletePlayerPhone(int playerId)
        {
            PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
            Plugin.Log.LogInfo("Deleting phone for player: " + playerController.name);
            PlayerPhone phone = playerController.transform.Find("PhonePrefab(Clone)").GetComponent<PlayerPhone>();
            string number = phoneNumberDict.FirstOrDefault(x => x.Value == phone.NetworkObjectId).Key;
            phone.GetComponent<NetworkObject>().RemoveOwnership();
            RemoveNumber(number);
        }

        public void DeletePhone(ulong phoneId)
        {
            Plugin.Log.LogInfo("Deleting phone with ID: " + phoneId);
            PhoneBehavior phone = GetNetworkObject(phoneId).GetComponent<PhoneBehavior>();

            string number = phoneNumberDict.FirstOrDefault(x => x.Value == phone.NetworkObjectId).Key;
            phone.GetComponent<NetworkObject>().RemoveOwnership();
            RemoveNumber(number);
        }

        public void RemoveNumber(string number)
        {
            if (number != null)
            {
                Plugin.Log.LogInfo("Removing number: " + number);

                phoneObjectDict.Remove(number);
                phoneNumberDict.Remove(number);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MakeOutgoingCallServerRpc(string number, ulong senderId, ServerRpcParams serverRpcParams = default)
        {
            string senderPhoneNumber = phoneNumberDict.FirstOrDefault(x => x.Value == senderId).Key;

            if (phoneNumberDict.ContainsKey(number))
            {
                // Successful call
                phoneObjectDict[number].RecieveCallClientRpc(senderId, senderPhoneNumber);
            }
            else
            {
                // No matching number, failed call
                phoneObjectDict[senderPhoneNumber].InvalidCallClientRpc("Invalid #");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptIncomingCallServerRpc(string number, ulong accepterId, ServerRpcParams serverRpcParams = default)
        {
            string accepterPhoneNumber = phoneNumberDict.FirstOrDefault(x => x.Value == accepterId).Key;

            phoneObjectDict[number].CallAcceptedClientRpc(accepterId, accepterPhoneNumber);
        }

        [ServerRpc(RequireOwnership = false)]
        public void HangUpCallServerRpc(string number, ulong cancellerId, ServerRpcParams serverRpcParams = default)
        {
            if (phoneNumberDict.ContainsKey(number))
            {
                string cancellerPhoneNumber = phoneNumberDict.FirstOrDefault(x => x.Value == cancellerId).Key;

                phoneObjectDict[number].HangupCallClientRpc(cancellerId, cancellerPhoneNumber);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void LineBusyServerRpc(string number, ServerRpcParams serverRpcParams = default)
        {
            phoneObjectDict[number].InvalidCallClientRpc("Line Busy");
        }
    }
}