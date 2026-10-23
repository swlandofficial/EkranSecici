import { Easing, interpolate, spring } from "remotion";
import { FPS } from "./theme";

/** Yumuşak, sekmesiz yay (0 → 1). */
export const ease = (frame: number, delay = 0, durationInFrames = 18) =>
  spring({ frame: frame - delay, fps: FPS, config: { damping: 200 }, durationInFrames });

/** Hafif sekmeli yay (giriş efektleri için). */
export const pop = (frame: number, delay = 0) =>
  spring({ frame: frame - delay, fps: FPS, config: { damping: 12, stiffness: 140, mass: 0.6 } });

export const lerp = (frame: number, input: number[], output: number[]) =>
  interpolate(frame, input, output, {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.inOut(Easing.cubic),
  });

/** Cümle i başladıktan sonra geçen kare (başlamadıysa negatif). */
export const since = (frame: number, from: number | undefined) => frame - (from ?? 0);

type Box = { x: number; y: number; w: number; h: number };
/** İki pencere konumu arasında ara değer. */
export const mixBox = (a: Box, b: Box, t: number): Box => ({
  x: a.x + (b.x - a.x) * t,
  y: a.y + (b.y - a.y) * t,
  w: a.w + (b.w - a.w) * t,
  h: a.h + (b.h - a.h) * t,
});
