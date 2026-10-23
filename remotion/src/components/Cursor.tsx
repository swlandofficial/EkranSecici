import React from "react";

/** Fare imleci; (x, y) imlecin ucudur. */
export const Cursor: React.FC<{ x: number; y: number; size?: number; pressed?: boolean }> = ({ x, y, size = 40, pressed }) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    style={{
      position: "absolute",
      left: x,
      top: y,
      transform: `scale(${pressed ? 0.85 : 1})`,
      transformOrigin: "0 0",
      filter: "drop-shadow(0 3px 6px rgba(0,0,0,.5))",
      zIndex: 50,
    }}
  >
    <path d="M3 2 L3 19 L8 14.5 L11.5 22 L14.5 20.7 L11 13.5 L18 13.5 Z" fill="#fff" stroke="#111" strokeWidth={1.3} strokeLinejoin="round" />
  </svg>
);
