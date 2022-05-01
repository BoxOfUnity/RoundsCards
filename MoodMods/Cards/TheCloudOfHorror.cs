using MoodMods.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace MoodMods.Cards
{
    class TheCloudOfHorror : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            gun.projectileSpeed = -0.5f;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.reloadTime = (99999 * 222);
            gun.ammo = 1;
            ObjectsToSpawn sawCloud = new ObjectsToSpawn() { };
            sawCloud.AddToProjectile = new GameObject("SawCloudSpawner", typeof(SawCloudSpawner));
            gun.objectsToSpawn = new ObjectsToSpawn[] { sawCloud };

        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //Run when the card is removed from the player
        }

        protected override string GetTitle()
        {
            return "The Cloud Of Horror";
        }
        protected override string GetDescription()
        {
            return "Vaguelly in the words of my cousin, Azupiranu, (creator of the Commitment Cards,) this card turns this game into a suspense horrer movie";
        }
        protected override GameObject GetCardArt()
        {
            return null;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                /*new CardInfoStat()
                {
                    positive = true,
                    stat = "Effect",
                    amount = "No",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }*/
                new CardInfoStat()
                {
                    positive = true,
                    stat = "For both of you.",
                    amount = "It's coming.",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = " to make it good.",
                    amount = "ONE CHANCE ",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DestructiveRed;
        }
        public override string GetModName()
        {
            return "MoodMods";
        }
    }
}
