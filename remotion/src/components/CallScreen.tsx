import React from "react";
import { colors } from "../theme";

/** Hasta çağrı ekranı içeriği (monitörün içine yerleşir, kapsayıcıya göre ölçeklenir). */
export const CallScreen: React.FC<{ number?: string; room?: string; scale?: number }> = ({
  number = "A-104",
  room = "Poliklinik 3",
  scale = 1,
}) => (
  <div
    style={{
      position: "absolute",
      inset: 0,
      background: colors.brandBright,
      color: colors.onBrand,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      gap: 8 * scale,
    }}
  >
    <div style={{ fontSize: 26 * scale, fontWeight: 800, letterSpacing: 2 * scale }}>HASTA ÇAĞRI</div>
    <div style={{ fontSize: 96 * scale, fontWeight: 900, lineHeight: 1 }}>{number}</div>
    <div style={{ fontSize: 26 * scale, fontWeight: 600 }}>{room}</div>
  </div>
);
