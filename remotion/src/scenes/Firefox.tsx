import React from "react";
import { useCurrentFrame } from "remotion";
import { ease, lerp } from "../anim";
import { Cursor } from "../components/Cursor";
import { DualMonitors, Win } from "../components/DualMonitors";
import { Split, usePanelSizes } from "../components/Split";
import { Backdrop, Card, Chip, FeatureHeader, Field } from "../components/Ui";
import { SceneProps } from "../timeline";
import { colors } from "../theme";

const ARGS = "--kiosk https://cagri.hastane.local";

/** Özellik 4: Firefox hasta çağrı ekranı şablonu. */
export const Firefox: React.FC<SceneProps> = ({ sentences }) => {
  const frame = useCurrentFrame();
  const sizes = usePanelSizes();
  const s0 = sentences[0];
  const s1 = sentences[1] ?? s0;

  const clickAt = s0.from + 26;
  const clicked = frame >= clickAt;
  const typed = Math.floor(lerp(frame, [clickAt + 6, clickAt + 36], [0, ARGS.length]));
  const kiosk = ease(frame, clickAt + 40, 16);

  // İmleç düğmeye gider ve tıklar
  const cx = lerp(frame, [s0.from, clickAt - 4], [sizes.card * 0.85, sizes.card * 0.55]);
  const cy = lerp(frame, [s0.from, clickAt - 4], [300, 70]);
  const pressed = frame >= clickAt - 2 && frame < clickAt + 5;

  // Personel kendi Firefox'unu açıp kapatır; çağrı ekranı etkilenmez
  const own = Math.min(ease(frame, s1.from + 4, 14), 1 - ease(frame, s1.to - 18, 12));

  const windows: Win[] = [
    { id: "own", kind: "browser", x: 0.12, y: 0.08, w: 0.76, h: 0.74, frame: true, opacity: own, scale: 0.9 + 0.1 * own },
    { id: "call", kind: "call", x: 1, y: 0, w: 1, h: 1, opacity: kiosk, scale: 0.92 + 0.08 * kiosk },
  ];

  return (
    <Backdrop>
      <Split
        header={<FeatureHeader index={4} title="Firefox çağrı ekranı şablonu" size={sizes.header} />}
        first={
          <Card width={sizes.card} zoom={sizes.cardZoom} style={{ position: "relative" }}>
            <div
              style={{
                fontSize: 26,
                fontWeight: 800,
                textAlign: "center",
                padding: "18px 20px",
                borderRadius: 16,
                marginBottom: 30,
                background: clicked ? colors.brandStrong : colors.brand,
                color: "#fff",
                transform: `scale(${pressed ? 0.97 : 1})`,
                boxShadow: clicked ? "none" : "0 10px 30px rgba(13,148,136,.4)",
              }}
            >
              Firefox hasta çağrı ekranı şablonu
            </div>
            <Field label="Program" size={24}>{clicked ? "firefox.exe" : ""}</Field>
            <Field label="Parametreler" size={24}>
              <span style={{ fontFamily: "monospace" }}>{ARGS.slice(0, Math.max(0, typed))}</span>
            </Field>
            <div style={{ display: "flex", gap: 12 }}>
              <Chip active={clicked} size={22}>Tam ekran</Chip>
              <Chip active={clicked} size={22}>Ayrı profil</Chip>
            </div>
            {frame < clickAt + 20 ? <Cursor x={cx} y={cy} size={46} pressed={pressed} /> : null}
          </Card>
        }
        second={
          <DualMonitors
            width={sizes.monitors}
            windows={windows}
            badge2={
              kiosk > 0.5 ? (
                <div style={{ background: colors.warning, color: colors.onBrand, fontWeight: 800, fontSize: 26, padding: "10px 18px", borderRadius: 12 }}>
                  Kiosk
                </div>
              ) : null
            }
          />
        }
      />
    </Backdrop>
  );
};
