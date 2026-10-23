import React from "react";
import { AbsoluteFill } from "remotion";
import { useLayout } from "../layout";

/**
 * Özellik sahnelerinin yerleşimi: başlık + iki panel.
 * Yatayda paneller yan yana, dikeyde alt alta dizilir.
 */
export const Split: React.FC<{ header: React.ReactNode; first: React.ReactNode; second: React.ReactNode }> = ({
  header,
  first,
  second,
}) => {
  const { vertical, content } = useLayout();
  return (
    <AbsoluteFill
      style={{
        top: content.top,
        left: content.left,
        width: content.right - content.left,
        height: content.bottom - content.top,
        display: "flex",
        flexDirection: "column",
        alignItems: vertical ? "center" : "stretch",
        gap: vertical ? 50 : 56,
      }}
    >
      {header}
      <div
        style={{
          flex: 1,
          display: "flex",
          flexDirection: vertical ? "column" : "row",
          alignItems: "center",
          justifyContent: vertical ? "center" : "space-between",
          gap: vertical ? 56 : 60,
        }}
      >
        {first}
        {second}
      </div>
    </AbsoluteFill>
  );
};

/** Panel boyutları (yatay / dikey) */
export const usePanelSizes = () => {
  const { vertical } = useLayout();
  // Dikeyde kart 1.2 kat büyütülür (zoom): metinler telefonda okunaklı olsun.
  return vertical
    ? { card: 800, cardZoom: 1.2, monitors: 1000, header: 58 }
    : { card: 640, cardZoom: 1, monitors: 1000, header: 60 };
};
