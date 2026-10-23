import narration from "../scripts/narration.json";
import { FPS } from "./theme";

export type SentenceTiming = { text: string; start: number; end: number };
export type SectionTiming = { id: SectionId; file: string; duration: number; sentences: SentenceTiming[] };
export type Timings = { sections: SectionTiming[] };

export const SECTION_IDS = ["intro", "reveal", "monitors", "autostart", "watchdog", "firefox", "noadmin", "outro"] as const;
export type SectionId = (typeof SECTION_IDS)[number];

/** Seslendirme sahne başlangıcından bu kadar sonra başlar ve bitince bu kadar beklenir (sn). */
export const LEAD = 0.35;
export const TAIL = 0.65;
/** Sahne geçişi (kare). Kuyruk sessizliğinden kısa olmalı ki ses üst üste binmesin. */
export const TRANSITION_FRAMES = 15;

/** Sahneye verilen cümle bilgisi (sahne başına göre kare). */
export type SceneSentence = { text: string; from: number; to: number };
export type SceneProps = { sentences: SceneSentence[]; duration: number };

export const sceneFrames = (s: SectionTiming) => Math.ceil((LEAD + s.duration + TAIL) * FPS);

export const toSceneSentences = (s: SectionTiming): SceneSentence[] =>
  s.sentences.map((x) => ({
    text: x.text,
    from: Math.round((LEAD + x.start) * FPS),
    to: Math.round((LEAD + x.end) * FPS),
  }));

/** Sahnelerin videodaki başlangıç kareleri (geçiş örtüşmeleri düşülmüş). */
export const sceneStarts = (sections: SectionTiming[]) => {
  const starts: number[] = [];
  let t = 0;
  sections.forEach((s, i) => {
    starts.push(t);
    t += sceneFrames(s) - (i < sections.length - 1 ? TRANSITION_FRAMES : 0);
  });
  return { starts, total: t };
};

/** Seslendirme henüz üretilmediyse metin uzunluğundan tahmini süre (stüdyoda çalışabilmek için). */
export const estimatedTimings = (): Timings => ({
  sections: narration.sections.map((section) => {
    let t = 0;
    const sentences = section.sentences.map((s, i) => {
      const start = t;
      t += s.text.length / 15.5;
      const r = { text: s.text, start, end: t };
      if (i < section.sentences.length - 1) t += "pauseAfter" in s ? Number(s.pauseAfter) : 0.35;
      return r;
    });
    return { id: section.id as SectionId, file: "", duration: t, sentences };
  }),
});
