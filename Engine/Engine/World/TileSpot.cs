﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace SE.World
{
    public class TileSpot
    {
        public TileMap TileMap { get; private set; }
        public TileChunk Chunk { get; internal set; }
        public List<TileTemplate> Tiles { get; internal set; } = new List<TileTemplate>();

        public bool IsActive => Tiles.Count > 0;

        public Point Position { get; private set; }
        public Vector2 WorldPosition => new Vector2(Position.X * TileMap.TileSize, Position.Y * TileMap.TileSize);

        public void Update(List<uint> tileIDs, TileChunk chunk, Point position)
        {
            DestroyTiles();
            Chunk = chunk;
            Position = position;
            for (int i = 0; i < tileIDs.Count; i++) {
                Tiles.Add(new TileTemplate(this, tileIDs[i]));
            }
            Instantiate();
        }

        public void AddTile(uint tileID)
        {
            if (!Contains(tileID)) {
                Tiles.Add(new TileTemplate(this, tileID));
            }
        }

        public void RemoveTile(uint tileID)
        {
            for (int i = 0; i < Tiles.Count; i++) {
                if (Tiles[i].TileID == tileID) {
                    Tiles[i].DestroyTile();
                    Tiles.Remove(Tiles[i]);
                }
            }
        }

        public void Instantiate()
        {
            if (IsActive)
                DestroyTiles();

            for (int i = 0; i < Tiles.Count; i++) {
                Tiles[i].Instantiate();
            }
        }

        public void DestroyTiles()
        {
            for (int i = 0; i < Tiles.Count; i++) {
                Tiles[i].DestroyTile();
            }
            Tiles.Clear();
        }

        public bool Contains(uint tileID)
        {
            for (int i = 0; i < Tiles.Count; i++) {
                if (Tiles[i].TileID == tileID) {
                    return true;
                }
            }
            return false;
        }

        public TileSpot(TileChunk chunk, Point position)
        {
            Position = position;
            Chunk = chunk;
            TileMap = chunk.TileMap;
        }
    }
}
