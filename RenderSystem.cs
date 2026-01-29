// <copyright file="NT_RenderSystem.cs" company="Luca Rager">
// Copyright (c) Luca Rager. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace CS2_Clearance {
    using System;
    #region Using Statements

    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Imaging;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using static Game.Rendering.OverlayRenderSystem;
    using Color = UnityEngine.Color;

    #endregion

    /// <summary>
    ///     Overlay Rendering System.
    /// </summary>
    public partial class RenderSystem : GameSystemBase {
        private EntityQuery         m_EdgeQuery;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private ProxyAction         m_ToggleOverlay;
        private bool                m_IsEnabled = false;

        /// <inheritdoc />
        protected override void OnCreate() {
            base.OnCreate();

            m_EdgeQuery = SystemAPI.QueryBuilder()
                                   .WithAll<Edge, Curve, Composition>()
                                   .WithNone<Deleted, Hidden>()
                                   .Build();

            m_ToggleOverlay                 = Mod.Instance.Settings.GetAction(nameof(Setting.ToggleOverlay));
            m_ToggleOverlay.shouldBeEnabled = true;

            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
        }

        /// <inheritdoc />
        protected override void OnUpdate() {
            if (m_ToggleOverlay.WasPressedThisFrame()) {
                m_IsEnabled = !m_IsEnabled;
            }

            if (!m_IsEnabled) {
                return;
            }

            try {
                if (Camera.main is null) {
                    throw new NullReferenceException("Camera.main is null");
                }
                var drawEdgesJob = new DrawEdgesJob
                      {
                          m_CurveTypeHandle          = SystemAPI.GetComponentTypeHandle<Curve>(),
                          m_CompositionTypeHandle    = SystemAPI.GetComponentTypeHandle<Composition>(),
                          m_RoadTypeHandle           = SystemAPI.GetComponentTypeHandle<Road>(),
                          m_SubwayTrackTypeHandle    = SystemAPI.GetComponentTypeHandle<SubwayTrack>(),
                          m_TramTrackTypeHandle      = SystemAPI.GetComponentTypeHandle<TramTrack>(),
                          m_TrainTrackTypeHandle     = SystemAPI.GetComponentTypeHandle<TrainTrack>(),
                          m_WaterwayTypeHandle       = SystemAPI.GetComponentTypeHandle<Waterway>(),
                          m_NetCompositionDataLookup = SystemAPI.GetComponentLookup<NetCompositionData>(),
                          m_EntityTypeHandle         = SystemAPI.GetEntityTypeHandle(),
                          m_Buffer                   = m_OverlayRenderSystem.GetBuffer(out var bufferJobHandle),
                          m_RoadHeight               = Mod.Instance.Settings.RoadHeight,
                          m_SubwayTrackHeight        = Mod.Instance.Settings.SubwayTrackHeight,
                          m_TramTrackHeight          = Mod.Instance.Settings.TramTrackHeight,
                          m_TrainTrackHeight         = Mod.Instance.Settings.TrainTrackHeight,
                          m_WaterwayHeight           = Mod.Instance.Settings.WaterwayHeight,
                      };

                var drawEdgesJobHandle = drawEdgesJob.ScheduleByRef(
                                                          m_EdgeQuery,
                                                          JobHandle.CombineDependencies(
                                                                                        Dependency,
                                                                                        bufferJobHandle
                                                                                       ));

                m_OverlayRenderSystem.AddBufferWriter(drawEdgesJobHandle);

                drawEdgesJobHandle.Complete();

                Dependency = drawEdgesJobHandle;
            } catch (Exception ex) {
                // ignored
            }
        }

        /// <summary>
        ///     Job to draw node overlays.
        /// </summary>
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        [BurstCompile]
        protected struct DrawEdgesJob : IJobChunk {
            [ReadOnly] public required ComponentTypeHandle<Curve>          m_CurveTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<Road>           m_RoadTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<SubwayTrack>    m_SubwayTrackTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<TramTrack>      m_TramTrackTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<TrainTrack>     m_TrainTrackTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<Waterway>       m_WaterwayTypeHandle;
            [ReadOnly] public required ComponentTypeHandle<Composition>    m_CompositionTypeHandle;
            [ReadOnly] public required ComponentLookup<NetCompositionData> m_NetCompositionDataLookup;
            [ReadOnly] public required EntityTypeHandle                    m_EntityTypeHandle;
            [ReadOnly] public required OverlayRenderSystem.Buffer          m_Buffer;
            [ReadOnly] public required float                               m_RoadHeight;
            [ReadOnly] public required float                               m_SubwayTrackHeight;
            [ReadOnly] public required float                               m_TramTrackHeight;
            [ReadOnly] public required float                               m_TrainTrackHeight;
            [ReadOnly] public required float                               m_WaterwayHeight;

            private bool IsValid(EdgeNodeGeometry nodeGeometry) {
                var @float = nodeGeometry.m_Left.m_Left.d   - nodeGeometry.m_Left.m_Left.a;
                var float2 = nodeGeometry.m_Left.m_Right.d  - nodeGeometry.m_Left.m_Right.a;
                var float3 = nodeGeometry.m_Right.m_Left.d  - nodeGeometry.m_Right.m_Left.a;
                var float4 = nodeGeometry.m_Right.m_Right.d - nodeGeometry.m_Right.m_Right.a;
                return math.lengthsq(@float + float2 + float3 + float4) > 1E-06f;
            }

            /// <inheritdoc />
            public void Execute(in ArchetypeChunk chunk,
                                int               unfilteredChunkIndex,
                                bool              useEnabledMask,
                                in v128           chunkEnabledMask) {
                var entitiesArray    = chunk.GetNativeArray(m_EntityTypeHandle);
                var curveArray       = chunk.GetNativeArray(ref m_CurveTypeHandle);
                var compositionArray = chunk.GetNativeArray(ref m_CompositionTypeHandle);

                for (var i = 0; i < entitiesArray.Length; i++) {
                    var composition        = compositionArray[i];
                    var netCompositionData = m_NetCompositionDataLookup[composition.m_Edge];
                    var curve              = curveArray[i];
                    var ceiling            = curve.m_Bezier;

                    var height = netCompositionData.m_HeightRange.max;

                    if (chunk.Has(ref m_RoadTypeHandle)) {
                        height = m_RoadHeight;
                    } else if (chunk.Has(ref m_SubwayTrackTypeHandle)) {
                        height = m_SubwayTrackHeight;
                    } else if
                        (chunk.Has(ref m_TramTrackTypeHandle)) {
                        height = m_TramTrackHeight;
                    } else if (chunk.Has(ref m_TrainTrackTypeHandle)) {
                        height = m_TrainTrackHeight;
                    } else if
                        (chunk.Has(ref m_WaterwayTypeHandle)) {
                        height = m_WaterwayHeight;
                    }

                    ceiling.y += height;

                    m_Buffer.DrawCurve(
                                       new Color(.1f, .5f, 1f, 1f),
                                       new Color(.1f, .5f, 1f, 0.2f),
                                       0.1f,
                                       StyleFlags.DepthFadeBelow,
                                       ceiling,
                                       netCompositionData.m_Width);
                }
            }
        }
    }
}
