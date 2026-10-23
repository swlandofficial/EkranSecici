import React from "react";
import { AbsoluteFill, Img, staticFile } from "remotion";
import { CallScreen } from "./components/CallScreen";
import { Monitor } from "./components/Monitor";
import { StaffDesktop } from "./components/StaffDesktop";
import { colors, fontFamily } from "./theme";

/** YouTube kapak resmi (1280x720). */
export const Thumbnail: React.FC = () => {
  return (
    <AbsoluteFill
      style={{
        fontFamily,
        background: `radial-gradient(circle at 78% 45%, rgba(45,212,191,.28), transparent 55%), ${colors.bg}`,
        color: colors.text,
      }}
    >
      {/* Sol: başlık */}
      <div style={{ position: "absolute", left: 64, top: 56, display: "flex", alignItems: "center", gap: 16 }}>
        <Img src={staticFile("logo.png")} style={{ width: 72, height: 72 }} />
        <div style={{ fontSize: 40, fontWeight: 800 }}>Ekran Seçici</div>
      </div>

      <div style={{ position: "absolute", left: 64, top: 190, width: 640 }}>
        <div style={{ fontSize: 76, fontWeight: 800, lineHeight: 1.02, letterSpacing: -2 }}>Her program</div>
        <div
          style={{
            fontSize: 104,
            fontWeight: 900,
            lineHeight: 1,
            letterSpacing: -3,
            color: colors.brandBright,
            marginTop: 6,
          }}
        >
          DOĞRU
          <br />
          EKRANDA
        </div>
        <div style={{ display: "flex", gap: 12, marginTop: 34 }}>
          {["Açılışta otomatik", "Yetki istemez", "Ücretsiz"].map((t, i) => (
            <div
              key={t}
              style={{
                fontSize: 26,
                fontWeight: 800,
                padding: "10px 18px",
                borderRadius: 999,
                background: i === 2 ? colors.warning : colors.brandSoft,
                color: i === 2 ? colors.onBrand : colors.brandStrong,
              }}
            >
              {t}
            </div>
          ))}
        </div>
      </div>

      {/* Sağ: iki monitör ve ok */}
      <Monitor width={300} style={{ position: "absolute", left: 700, top: 300 }}>
        <StaffDesktop scale={0.6} />
      </Monitor>
      <Monitor width={460} variant="brand" style={{ position: "absolute", left: 790, top: 70 }}>
        <CallScreen scale={1.05} />
      </Monitor>
      {/* Ok: personel ekranından çağrı ekranına */}
      <svg
        width="190"
        height="110"
        viewBox="0 0 190 110"
        style={{
          position: "absolute",
          left: 975,
          top: 455,
          transform: "rotate(-32deg)",
          filter: "drop-shadow(0 8px 20px rgba(0,0,0,.55))",
        }}
      >
        <path d="M0 40 H120 V5 L190 55 L120 105 V70 H0 Z" fill={colors.brandBright} />
      </svg>
      {/* Monitör numaraları */}
      {[
        { n: "1", left: 690, top: 290 },
        { n: "2", left: 772, top: 52 },
      ].map(({ n, left, top }) => (
        <div
          key={n}
          style={{
            position: "absolute",
            left,
            top,
            width: 56,
            height: 56,
            borderRadius: 999,
            background: n === "2" ? colors.brandBright : colors.monitorScreen,
            color: colors.onBrand,
            fontSize: 32,
            fontWeight: 900,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            border: `4px solid ${colors.bg}`,
          }}
        >
          {n}
        </div>
      ))}
    </AbsoluteFill>
  );
};
