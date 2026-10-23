import React from "react";

type P = { size: number; color: string; strokeWidth?: number };
const Svg: React.FC<P & { children: React.ReactNode }> = ({ size, color, strokeWidth = 2, children }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth={strokeWidth}
    strokeLinecap="round" strokeLinejoin="round">
    {children}
  </svg>
);

export const CheckIcon: React.FC<P> = (p) => <Svg {...p}><path d="M5 12.5l4.5 4.5L19 7.5" /></Svg>;
export const RefreshIcon: React.FC<P> = (p) => (
  <Svg {...p}><path d="M21 12a9 9 0 1 1-3-6.7L21 8" /><path d="M21 3v5h-5" /></Svg>
);
export const MagnetIcon: React.FC<P> = (p) => (
  <Svg {...p}><rect x="3" y="11" width="18" height="10" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></Svg>
);
export const PinIcon: React.FC<P> = (p) => (
  <Svg {...p}><path d="M12 17v5" /><path d="M5 17h14l-2-4V5H7v8z" /></Svg>
);
export const ShieldIcon: React.FC<P> = (p) => (
  <Svg {...p}><path d="m12 3 8 4v5c0 5-3.5 8-8 9-4.5-1-8-4-8-9V7z" /><path d="m9 12 2 2 4-4" /></Svg>
);
export const PowerIcon: React.FC<P> = (p) => (
  <Svg {...p}><path d="M12 2v10" /><path d="M18.4 6.6a9 9 0 1 1-12.8 0" /></Svg>
);
export const DownloadIcon: React.FC<P> = (p) => (
  <Svg {...p}><path d="M12 3v12" /><path d="m7 10 5 5 5-5" /><path d="M5 21h14" /></Svg>
);
export const CloseIcon: React.FC<P> = (p) => <Svg {...p}><path d="M6 6l12 12M18 6 6 18" /></Svg>;
