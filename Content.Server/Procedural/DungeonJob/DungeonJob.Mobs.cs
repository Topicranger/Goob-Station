// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Content.Server.Ghost.Roles.Components;
using Content.Server.NPC.Systems;
using Content.Shared.Physics;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Content.Shared.Storage;
using Robust.Shared.Collections;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    /// <summary>
    /// <see cref="MobsDunGen"/>
    /// </summary>
    private async Task PostGen(
        MobsDunGen gen,
        Dungeon dungeon,
        Random random)
    {
        var availableRooms = new ValueList<DungeonRoom>();
        availableRooms.AddRange(dungeon.Rooms);
        var availableTiles = new ValueList<Vector2i>(dungeon.AllTiles);
        var contents = _prototype.Index(gen.Contents);

        var count = random.Next(gen.MinCount, gen.MaxCount + 1);
        var npcs = _entManager.System<NPCSystem>();

        for (var i = 0; i < count; i++)
        {
            while (availableTiles.Count > 0)
            {
                var tile = availableTiles.RemoveSwap(random.Next(availableTiles.Count));

                if (!_anchorable.TileFree(_grid, tile, (int) CollisionGroup.MachineLayer,
                        (int) CollisionGroup.MachineLayer))
                {
                    continue;
                }

                var entities = _entTable.GetSpawns(contents, random);

                foreach (var ent in entities)
                {
                    var uid = _entManager.SpawnAtPosition(ent, _maps.GridTileToLocal(_gridUid, _grid, tile));
                    _entManager.RemoveComponent<GhostRoleComponent>(uid);
                    _entManager.RemoveComponent<GhostTakeoverAvailableComponent>(uid);
                    npcs.SleepNPC(uid);
                }

                break;
            }

            await SuspendDungeon();

            if (!ValidateResume())
                return;
        }
    }
}