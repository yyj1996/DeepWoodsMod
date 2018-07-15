﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWoodsMod
{
    /// <summary>
    /// Heh, this is the most literal easter egg I've ever written :D
    /// </summary>
    class EasterEgg : TerrainFeature
    {
        // public const string FESTIVAL_TILESHEET_ID = "Festivals";
        // this.map.AddTileSheet(new TileSheet(FESTIVAL_TILESHEET_ID, this.map, "Maps\\Festivals", new Size(32, 32), new Size(16, 16)));

        private Texture2D texture;
        private int eggTileIndex;
        private bool wasPickedUp;

        public EasterEgg()
        {
            this.texture = Game1.content.Load<Texture2D>("Maps\\Festivals");
            this.eggTileIndex = Game1.random.Next(67, 71);
            this.wasPickedUp = false;
        }

        public override Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        public override bool isPassable(Character c = null)
        {
            return this.wasPickedUp;
        }

        public override bool performUseAction(Vector2 tileLocation, GameLocation location)
        {
            if (this.wasPickedUp)
                return false;

            if (Game1.player.addItemToInventoryBool(new EasterEggItem(), false))
            {
                if (!(Game1.player.FarmerSprite is DeepWoodsMod.FarmerSprite))
                {
                    Game1.player.FarmerSprite = new DeepWoodsMod.FarmerSprite(Game1.player.FarmerSprite);
                }
                Game1.player.animateOnce(StardewValley.FarmerSprite.harvestItemUp + Game1.player.FacingDirection);
                Game1.player.canMove = false;
                Game1.player.currentLocation.playSound("coin");
                this.wasPickedUp = true;
                return true;
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                return false;
            }
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
        {
            return false;
        }

        public override void draw(SpriteBatch b, Vector2 tileLocation)
        {
            if (this.wasPickedUp)
                return;

            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64, tileLocation.Y * 64));

            Rectangle destinationRectangle = new Rectangle((int)local.X, (int)local.Y, 64, 64);
            Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(this.texture, this.eggTileIndex, 16, 16);

            b.Draw(this.texture, destinationRectangle, sourceRectangle, Color.White);
        }
    }
}