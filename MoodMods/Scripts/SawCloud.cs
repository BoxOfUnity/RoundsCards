using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnboundLib.Utils;
using System.Reflection;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using MoodMods.Extensions;
using System.Collections;
using HarmonyLib;

namespace MoodMods.Scripts
{
    class ShockBlastAssets
    {
        private static GameObject _shockblast = null;

        internal static GameObject shockblast
        {
            get
            {
                if (_shockblast != null) { return _shockblast; }
                else
                {
                    _shockblast = new GameObject("Shock_blast", typeof(ShockBlastEffect), typeof(PhotonView));
                    UnityEngine.GameObject.DontDestroyOnLoad(_shockblast);

                    return _shockblast;
                }
            }
            set { }
        }


    }

    public class ShockBlastSpawner : MonoBehaviour
    {
        private static bool Initialized = false;

        void Awake()
        {
            if (!Initialized)
            {
                PhotonNetwork.PrefabPool.RegisterPrefab(ShockBlastAssets.shockblast.name, ShockBlastAssets.shockblast);
            }
        }

        void Start()
        {
            MoodMods.Log("ShockBlastSpawner Start");
            if (!Initialized)
            {
                Initialized = true;
                return;
            }
            MoodMods.Log("Checking is projectile is mine?");
            if (!PhotonNetwork.OfflineMode && !this.gameObject.transform.parent.GetComponent<ProjectileHit>().ownPlayer.data.view.IsMine) return;

            MoodMods.Log("Instantiating object");
            var name = ShockBlastAssets.shockblast.name;
            MoodMods.Log("Name retrieved " + name);
            PhotonView photonView = gameObject.transform.parent.GetComponent<PhotonView>(); ;
            if (!photonView)
            {
                MoodMods.Log("PhotonView is null, aborting start");
                return;
            }

            MoodMods.Log("PhotonView: " + photonView);
            int viewId = photonView.ViewID;
            MoodMods.Log("ViewId retrieved " + viewId);
            PhotonNetwork.Instantiate(
                name,
                transform.position,
                transform.rotation,
                0,
                new object[] { viewId }
            );
        }
    }

    [RequireComponent(typeof(PhotonView))]
    public class ShockBlastEffect : MonoBehaviour, IPunInstantiateMagicCallback
    {
        private Player player;
        private Gun gun;
        private ProjectileHit projectile;
        public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
        {

            object[] instantiationData = info.photonView.InstantiationData;
            GameObject parent = PhotonView.Find((int)instantiationData[0]).gameObject;
            gameObject.transform.SetParent(parent.transform);
            MoodMods.Log("Photon instantiate for shockblasteffect");
            foreach (Transform child in gameObject.transform.parent)
            {
                MoodMods.Log("Children of parent: " + child.gameObject);
            }
            projectile = gameObject.transform.parent.GetComponent<ProjectileHit>();
            foreach (Transform child in projectile.transform)
            {
                MoodMods.Log("Children of projectile: " + child.gameObject);
            }
            player = projectile.ownPlayer;

            gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
        }

        void Awake()
        {

        }
        void Start()
        {
            MoodMods.Log("Starting shockBlastEffect");
            var visual = GenerateVisual();
            var range = CalculateRange(gun);
            MoodMods.Log("Visual generated, triggering");
            TriggerVisual(visual, range);

            MoodMods.Log("Getting targets");
            var targetsInRange = GetInRangeTargets(player.transform.position, range);
            MoodMods.Log("Beginning explosion logic");
            foreach (Collider2D target in targetsInRange)
            {
                MoodMods.Log("Checking for rigidbody on collider " + target + " from attachedRigidbody: " + target.attachedRigidbody + " from component in parent: " + target.GetComponentInParent<Rigidbody2D>());
                var rigidbody = target.GetComponentInParent<Rigidbody2D>();
                if (rigidbody)
                {
                    MoodMods.Log("rigidbody found");
                    DoPushRigidbody(player.transform.position, rigidbody, gun.GetAdditionalData().shockBlastBaseForce * (gun.damage / 2));
                }
                else
                {
                    MoodMods.Log("Checking for player on collider " + target + ": " + target.GetComponentInParent<Player>());
                    var otherPlayer = target.GetComponentInParent<Player>();
                    if (otherPlayer)
                    {
                        MoodMods.Log("player found");
                        DoPushCharData(player.transform.position, otherPlayer, gun.GetAdditionalData().shockBlastBaseForce * (gun.damage / 2));
                    }
                }
                MoodMods.Log("Checking for damageable on collider " + target);
                var damageable = target.transform.gameObject.GetComponent<Damagable>();
                if (damageable)
                {
                    MoodMods.Log("damageable found");
                    DoDamage(target.GetComponent<Damagable>());
                }
            }

            projectile.projectileColor = Color.black;
            projectile.bulletCanDealDeamage = false;
            projectile.sendCollisions = false;
            projectile.transform.position = (new Vector3(-1000f, -10000f, -1000f));
        }

        private ISet<Collider2D> GetInRangeTargets(Vector2 origin, float range)
        {
            ISet<Collider2D> targets = new HashSet<Collider2D>();
            var colliders = Physics2D.OverlapCircleAll(origin, range);
            var playerCollider = player.GetComponent<Collider2D>();
            MoodMods.Log("Player collider? " + playerCollider + "At position " + player.transform.position);
            foreach (Collider2D collider in colliders)
            {
                MoodMods.Log("Looking at collider (" + collider + ") for gameobject " + collider.gameObject + " at position " + collider.transform.position);
                if (!collider.Equals(player.GetComponent<Collider2D>()))
                {
                    MoodMods.Log("Checking if collider (" + collider + ") for gameobject " + collider.gameObject + " is visible");
                    //Eliminates encountered colliders for anything without a rigidbody except players, which apparently don't have one. Go figure.
                    var list = Physics2D.RaycastAll(origin, (((Vector2)collider.transform.position) - origin).normalized, range).Select(item => item.collider).Where(item => !item.Equals(playerCollider) && (item.attachedRigidbody || (item.GetComponentInParent<Player>() && item.GetComponentInParent<Player>().playerID != player.playerID))).ToList();
                    list.ForEach(item => MoodMods.Log("raycast item: " + item.gameObject + "is the collider we're looking at? " + (item.Equals(collider))));
                    if (list.Count > 0 && list[0].Equals(collider))
                    {
                        MoodMods.Log("Item matched, adding to targets: " + collider.transform.gameObject);
                        targets.Add(collider);
                    }
                }
            }
            return targets;
        }

        private void DoPushRigidbody(Vector2 origin, Rigidbody2D rigidbody, float force)
        {
            MoodMods.Log("Doing push");
            MoodMods.Log("Adding force " + force + " in direction " + (rigidbody.position - origin).normalized + "For a net value of " + ((rigidbody.position - origin).normalized * force * rigidbody.mass));
            rigidbody.AddForce((rigidbody.position - origin).normalized * force * rigidbody.mass * 0.75f);
        }

        private void DoPushCharData(Vector2 origin, Player otherPlayer, float force)
        {
            MoodMods.Log("Doing push for player");
            MoodMods.Log(" adding force " + force + " for total vector " + ((Vector2)otherPlayer.transform.position - origin).normalized * force * 2);
            var healthHandler = otherPlayer.GetComponentInChildren<HealthHandler>();
            healthHandler.CallTakeForce(((Vector2)otherPlayer.transform.position - origin).normalized * force * 2);
            MoodMods.Log("Post force");
        }

        private void DoDamage(Damagable damageable)
        {

            MoodMods.Log("Doing damage");
            var totalDamage = Vector2.up * 55 * gun.damage * gun.bulletDamageMultiplier / 1.5f;
            MoodMods.Log("Gun damage: " + gun.damage + " bulletDamageMultiplier " + gun.bulletDamageMultiplier + " Total damage: " + totalDamage + " transform? " + player.transform);
            damageable.CallTakeDamage(Vector2.up * 55 * gun.damage * gun.bulletDamageMultiplier / 1.5f, player.transform.position);
        }


        private GameObject GenerateVisual()
        {
            GameObject _shockblastVisual;
            List<CardInfo> activecards = ((ObservableCollection<CardInfo>)typeof(CardManager).GetField("activeCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).ToList();
            List<CardInfo> inactivecards = (List<CardInfo>)typeof(CardManager).GetField("inactiveCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            List<CardInfo> allcards = activecards.Concat(inactivecards).ToList();
            GameObject E_Overpower = allcards.Where(card => card.cardName.ToLower() == "overpower").First().GetComponent<CharacterStatModifiers>().AddObjectToPlayer.GetComponent<SpawnObjects>().objectToSpawn[0];
            _shockblastVisual = UnityEngine.GameObject.Instantiate(E_Overpower, new Vector3(0, 100000f, 0f), Quaternion.identity);
            _shockblastVisual.name = "E_Discombobulate";
            DontDestroyOnLoad(_shockblastVisual);
            foreach (ParticleSystem parts in _shockblastVisual.GetComponentsInChildren<ParticleSystem>())
            {
                parts.startColor = Color.cyan;
                parts.startLifetime = parts.startLifetime / 2;
            }
            _shockblastVisual.transform.GetChild(1).GetComponent<LineEffect>().colorOverTime.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f) };
            UnityEngine.GameObject.Destroy(_shockblastVisual.transform.GetChild(2).gameObject);
            _shockblastVisual.transform.GetChild(1).GetComponent<LineEffect>().offsetMultiplier = 0f;
            _shockblastVisual.transform.GetChild(1).GetComponent<LineEffect>().playOnAwake = true;
            UnityEngine.GameObject.Destroy(_shockblastVisual.GetComponent<FollowPlayer>());
            _shockblastVisual.GetComponent<DelayEvent>().time = 0f;
            UnityEngine.GameObject.Destroy(_shockblastVisual.GetComponent<SoundImplementation.SoundUnityEventPlayer>());
            UnityEngine.GameObject.Destroy(_shockblastVisual.GetComponent<Explosion>());
            UnityEngine.GameObject.Destroy(_shockblastVisual.GetComponent<Explosion_Overpower>());
            UnityEngine.GameObject.Destroy(_shockblastVisual.GetComponent<RemoveAfterSeconds>());
            return _shockblastVisual;
        }

        private void TriggerVisual(GameObject visual, float range)
        {
            MoodMods.Log("Setting scale");
            visual.transform.localScale = new Vector3(1f, 1f, 1f);
            MoodMods.Log("Adding removeAfterSeconds");
            visual.AddComponent<RemoveAfterSeconds>().seconds = 5f;
            MoodMods.Log("Initializing line effect");
            visual.transform.GetChild(1).GetComponent<LineEffect>().SetFieldValue("inited", false);
            typeof(LineEffect).InvokeMember("Init",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null, visual.transform.GetChild(1).GetComponent<LineEffect>(), new object[] { });
            MoodMods.Log("Adjusting line effect");
            visual.transform.GetChild(1).GetComponent<LineEffect>().radius = (range);
            visual.transform.GetChild(1).GetComponent<LineEffect>().SetFieldValue("startWidth", 0.5f);
            visual.transform.position = player.transform.position;
            MoodMods.Log("Playing effect");
            visual.transform.GetChild(1).GetComponent<LineEffect>().Play();
        }
        private float CalculateRange(Gun gun)
        {
            var range = gun.GetAdditionalData().shockBlastRange + ((float)Math.Sqrt(gun.projectileSpeed) * 1.2f);
            MoodMods.Log("gun.projectileSpeed: " + gun.projectileSpeed);
            MoodMods.Log("Range: " + range);
            return range;
        }


    }
}