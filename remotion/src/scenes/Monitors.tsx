import React from "react";
import { useCurrentFrame } from "remotion";
import { ease, mixBox } from "../anim";
import { DualMonitors } from "../components/DualMonitors";
import { Split, usePanelSizes } from "../components/Split";
import { Backdrop, Card, Chip, FeatureHeader, Field } from "../components/Ui";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

const MODES = ["Normal pencere", "Ekranı kapla", "Tam ekran"] as const;

/** Özellik 1: monitör ve pencere modu seçimi. */
export const Monitors: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const sizes = usePanelSizes();
  const s0 = sentences[0];
  const s1 = sentences[1] ?? s0;

  // Pencere 1. ekrandan 2. ekrana geçer…
  const move = ease(frame, s0.from + 12, 30);
  // …sonra modlar sırayla uygulanır.
  const len = s1.to - s1.from;
  const m1 = ease(frame, s1.from + len * 0.33, 14);
  const m2 = ease(frame, s1.from + len * 0.7, 14);
  const mode = m2 > 0.5 ? 2 : m1 > 0.5 ? 1 : 0;

  const start = { x: 0.3, y: 0.2, w: 0.5, h: 0.5 };
  const normal = { x: 1.19, y: 0.19, w: 0.62, h: 0.62 };
  const full = { x: 1, y: 0, w: 1, h: 1 };
  let box = mixBox(start, normal, move);
  box = mixBox(box, full, m1);

  const monitorPulse = Math.max(0, Math.sin(((frame - s0.from) / 30) * Math.PI)) * (frame < s1.from ? 1 : 0);

  return (
    <Backdrop>
      <Split
        header={<FeatureHeader index={1} title="Monitör ve pencere modu" size={sizes.header} />}
        first={
          <Card width={sizes.card} zoom={sizes.cardZoom}>
            <Field label="Program">firefox.exe — Hasta çağrı</Field>
            <Field label="Monitör" highlight={monitorPulse}>
              <span style={{ color: colors.brandBright }}>●</span> Monitör 2 — Bekleme salonu
              <span style={{ marginLeft: "auto", color: colors.muted }}>▾</span>
            </Field>
            <div style={{ fontSize: 21, color: colors.muted, fontWeight: 600, marginBottom: 10 }}>Pencere modu</div>
            <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
              {MODES.map((m, i) => (
                <Chip key={m} active={frame >= s1.from - 4 && i === mode} size={24}>
                  {m}
                </Chip>
              ))}
            </div>
          </Card>
        }
        second={
          <DualMonitors
            width={sizes.monitors}
            windows={[{ id: "call", kind: "call", ...box, frame: mode < 2 }]}
          />
        }
      />
    </Backdrop>
  );
};
