﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using static DeepWoodsMod.DeepWoodsRandom;
using static DeepWoodsMod.DeepWoodsSettings;

namespace DeepWoodsMod
{
    class GingerBreadHouse : ResourceClump
    {
        private new int parentSheetIndex;
        private NetFloat nextSpawnFoodHealth = new NetFloat();

        public GingerBreadHouse()
            : base()
        {
            InitNetFields();
            this.parentSheetIndex = 0;
        }

        public GingerBreadHouse(Vector2 tile)
            : base(602, 5, 3, tile)
        {
            InitNetFields();
            this.parentSheetIndex = 0;
            this.health.Value = Settings.Objects.GingerBreadHouse.Health;
            this.nextSpawnFoodHealth.Value = Settings.Objects.GingerBreadHouse.Health - Settings.Objects.GingerBreadHouse.DamageIntervalForFoodDrop;
        }

        private void InitNetFields()
        {
            this.NetFields.AddFields(this.nextSpawnFoodHealth);
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            Vector2 globalPosition = this.tile.Value * 64f;
            if (this.shakeTimer > 0)
            {
                globalPosition.X += (float)Math.Sin(2.0 * Math.PI / this.shakeTimer) * 4f;
            }

            Rectangle upperHousePartRectangle = Game1.getSourceRectForStandardTileSheet(Textures.gingerbreadHouse, this.parentSheetIndex, 16, 16);
            upperHousePartRectangle.Width = 5 * 16;
            upperHousePartRectangle.Height = 4 * 16;

            Rectangle bottomHousePartRectangle = Game1.getSourceRectForStandardTileSheet(Textures.gingerbreadHouse, this.parentSheetIndex, 16, 16);
            bottomHousePartRectangle.Y += 4 * 16;
            bottomHousePartRectangle.Width = 5 * 16;
            bottomHousePartRectangle.Height = 3 * 16;

            Vector2 upperHousePartPosition = globalPosition;
            upperHousePartPosition.Y -= 4 * 64;

            spriteBatch.Draw(Textures.gingerbreadHouse, Game1.GlobalToLocal(Game1.viewport, upperHousePartPosition), upperHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.tile.Y + 1f) * 64f / 10000f + this.tile.X / 100000f));
            spriteBatch.Draw(Textures.gingerbreadHouse, Game1.GlobalToLocal(Game1.viewport, globalPosition), bottomHousePartRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, ((this.tile.Y + 1f) * 64f / 10000f + this.tile.X / 100000f));
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            if (!(t is Axe))
                return false;

            if (t.upgradeLevel < Settings.Objects.GingerBreadHouse.MinimumAxeLevel)
            {
                location.playSound("axe");
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
                Game1.player.jitterStrength = 1f;
                return false;
            }

            Vector2 debrisLocation = CalculateDebrisLocation(tileLocation);

            location.playSound("axchop");
            Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)debrisLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), false, -1, false, -1);
            this.health.Value -= Math.Max(1f, (t.upgradeLevel + 1) * 0.75f);

            if (this.health > 0)
            {
                if (this.health <= this.nextSpawnFoodHealth)
                {
                    location.playSound("stumpCrack");

                    t.getLastFarmerToUse().gainExperience(Farmer.foragingSkill, 25);

                    SpawnFoodItem(location as DeepWoods, t, (int)tileLocation.X, (int)tileLocation.Y);

                    this.nextSpawnFoodHealth.Value = this.health - Settings.Objects.GingerBreadHouse.DamageIntervalForFoodDrop;
                }

                this.shakeTimer = 100f;
                return false;
            }

            PlayDestroyedSounds(location);

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    SpawnFoodItems(location as DeepWoods, t, (int)(this.tile.X + x), (int)(this.tile.Y + y));
                    Game1.createRadialDebris(Game1.currentLocation, Debris.woodDebris, (int)(this.tile.X + x), (int)(this.tile.Y + y), Game1.random.Next(4, 9), false, -1, false, -1);
                }
            }

            return true;
        }

        private int GetRandomFoodType(DeepWoods deepWoods)
        {
            DeepWoodsRandom random = new DeepWoodsRandom(deepWoods, (deepWoods?.GetSeed() ?? Game1.random.Next()) ^ Game1.currentGameTime.TotalGameTime.Milliseconds ^ (int)this.tile.X ^ (int)this.tile.Y);
            random.EnterMasterMode();

            return random.GetRandomValue(new WeightedInt[] {
                // ITEM NAME // SELL PRICE // WEIGHT
                CreateWeightedValueForFootType(245), // Sugar               //  50 // 2000
                CreateWeightedValueForFootType(246), // Wheat Flour         //  50 // 2000
                CreateWeightedValueForFootType(229), // Tortilla            //  50 // 2000
                CreateWeightedValueForFootType(216), // Bread               //  60 // 1666
                CreateWeightedValueForFootType(223), // Cookie              // 140 //  714
                CreateWeightedValueForFootType(234), // Blueberry Tart      // 150 //  666
                CreateWeightedValueForFootType(220), // Chocolate Cake      // 200 //  500
                CreateWeightedValueForFootType(243), // Miner's Treat       // 200 //  500
                CreateWeightedValueForFootType(203), // Strange Bun         // 225 //  444
                CreateWeightedValueForFootType(651), // Poppyseed Muffin    // 250 //  400
                CreateWeightedValueForFootType(611), // Blackberry Cobbler  // 260 //  384
                CreateWeightedValueForFootType(607), // Roasted Hazelnuts   // 270 //  370
                CreateWeightedValueForFootType(731), // Maple Bar           // 300 //  333
                CreateWeightedValueForFootType(608), // Pumpkin Pie         // 385 //  259
                CreateWeightedValueForFootType(222), // Rhubarb Pie         // 400 //  250
                CreateWeightedValueForFootType(221), // Pink Cake           // 480 //  208
                // Non-food items with hardcoded weight (their price is too low, they would always spawn)
                new WeightedInt(388, 3000),  // Wood // 2 // 3000 (50000)
                new WeightedInt(92, 3000),   // Sap  // 2 // 3000 (50000)
            });
        }

        private WeightedInt CreateWeightedValueForFootType(int type)
        {
            int price = 0;
            if (Game1.objectInformation.ContainsKey(type))
            {
                price = Convert.ToInt32(Game1.objectInformation[type].Split('/')[StardewValley.Object.objectInfoPriceIndex]);
            }
            // We invert the price to get higher weights for cheaper items and vice versa.
            return new WeightedInt(type, 100000 / price);
        }

        private void SpawnFoodItems(DeepWoods deepWoods, Tool t, int x, int y)
        {
            for (int i = 0, n = Game1.random.Next(1,4); i < n; i++)
            {
                SpawnFoodItem(deepWoods, t, x, y);
            }
        }

        private void SpawnFoodItem(DeepWoods deepWoods, Tool t, int x, int y)
        {
            if (Game1.IsMultiplayer)
                Game1.createMultipleObjectDebris(GetRandomFoodType(deepWoods), x, y, 1, t.getLastFarmerToUse().UniqueMultiplayerID);
            else
                Game1.createMultipleObjectDebris(GetRandomFoodType(deepWoods), x, y, 1);
        }

        private void PlayDestroyedSounds(GameLocation location)
        {
            DelayedAction.playSoundAfterDelay("stumpCrack", 0, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 10, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 20, location);
            DelayedAction.playSoundAfterDelay("stumpCrack", 50, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 60, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 70, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 110, location);
            DelayedAction.playSoundAfterDelay("breakingGlass", 120, location);
            DelayedAction.playSoundAfterDelay("boulderBreak", 160, location);

            DelayedAction.playSoundAfterDelay("cacklingWitch", 2000, location);
        }

        private Vector2 CalculateDebrisLocation(Vector2 tileLocation)
        {
            int xOffset = Game1.random.Next(0, 2);
            int yOffset = Game1.random.Next(0, 2);

            Vector2 debrisLocation = new Vector2(tileLocation.X, tileLocation.Y);

            if ((tileLocation.X + xOffset) > (this.tile.X + this.width - 1))
            {
                debrisLocation.X = debrisLocation.X - xOffset;
            }
            else if ((tileLocation.X - xOffset) < this.tile.X)
            {
                debrisLocation.X = debrisLocation.X + xOffset;
            }
            else
            {
                debrisLocation.X = debrisLocation.X + (Game1.random.NextDouble() < 0.5 ? xOffset : -xOffset);
            }

            if ((tileLocation.Y + yOffset) > (this.tile.Y + this.height - 1))
            {
                debrisLocation.Y = debrisLocation.Y - yOffset;
            }
            else if ((tileLocation.Y - yOffset) < this.tile.Y)
            {
                debrisLocation.Y = debrisLocation.Y + yOffset;
            }
            else
            {
                debrisLocation.Y = debrisLocation.Y + (Game1.random.NextDouble() < 0.5 ? yOffset : -yOffset);
            }

            return debrisLocation;
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            return true;
        }
    }
}
