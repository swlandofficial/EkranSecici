import React from "react";
import { useCurrentFrame } from "remotion";
import { ease, lerp } from "../anim";
import { DualMonitors } from "../components/DualMonitors";
import { CheckIcon, PowerIcon } from "../components/Icons";
import { Split, usePanelSizes } from "../components/Split";
import { Backdrop, Card, FeatureHeader } from "../components/Ui";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

const Step: React.FC<{ text: string; t: number; waiting?: boolean; frame: number }> = ({ text, t, waiting, frame }) => (
  <div
    style={{
      display: "flex",
      alignItems: "center",
      gap: 18,
      fontSize: 28,
      fontWeight: 600,
      marginBottom: 20,
      opacity: t,
      transform: `translateX(${(1 - t) * -30}px)`,
    }}
  >
    <div
      style={{
        width: 44,
        height: 44,
        borderRadius: 999,
        flexShrink: 0,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: waiting ? "transparent" : colors.brandBright,
        border: waiting ? `4px solid ${colors.line}` : "none",
        borderTopColor: waiting ? colors.warning : undefined,
        transform: waiting ? `rotate(${frame * 12}deg)` : undefined,
      }}
    >
      {waiting ? null : <CheckIcon size={26} color={colors.onBrand} strokeWidth={3} />}
    </div>
    <span style={{ color: waiting ? colors.warning : colors.text }}>{text}</span>
  </div>
);

/** Özellik 2: açılışta otomatik, dış ekranı bekler. */
export const Autostart: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const sizes = usePanelSizes();
  const s0 = sentences[0];
  const s1 = sentences[1] ?? s0;
  const len1 = s1.to - s1.from;

  const booted = frame > s0.from + 14;
  const tvOnAt = s1.from + len1 * 0.45;
  const tvOn = lerp(frame, [tvOnAt, tvOnAt + 10], [0, 1]);
  const callIn = ease(frame, tvOnAt + 10, 16);
  const connected = frame >= tvOnAt;

  const steps = [
    { text: "Bilgisayar açıldı", at: s0.from + 4 },
    { text: "Ekran Seçici başladı", at: s0.from + 22 },
    { text: connected ? "Monitör 2 bağlandı" : "Monitör 2 bekleniyor…", at: s1.from + 2, waiting: !connected },
    { text: "Çağrı ekranı açıldı", at: tvOnAt + 16 },
  ];

  return (
    <Backdrop>
      <Split
        header={<FeatureHeader index={2} title="Açılışta otomatik" size={sizes.header} />}
        first={
          <Card width={sizes.card} zoom={sizes.cardZoom}>
            <div style={{ display: "flex", alignItems: "center", gap: 14, marginBottom: 28, color: colors.muted, fontSize: 24, fontWeight: 700 }}>
              <PowerIcon size={30} color={colors.brandBright} /> PC AÇILIŞI
            </div>
            {steps.map((s) => (
              <Step key={s.at} text={s.text} t={ease(frame, s.at, 12)} waiting={s.waiting} frame={frame} />
            ))}
          </Card>
        }
        second={
          <DualMonitors
            width={sizes.monitors}
            screen1={booted ? "desktop" : "boot"}
            screen2On={tvOn}
            windows={[{ id: "call", kind: "call", x: 1, y: 0, w: 1, h: 1, opacity: callIn, scale: 0.92 + 0.08 * callIn }]}
          />
        }
      />
    </Backdrop>
  );
};
