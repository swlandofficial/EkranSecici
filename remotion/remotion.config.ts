import fs from "node:fs";
import path from "node:path";
import { Config } from "@remotion/cli/config";

// Hazır kurulu Chromium'u kullan (npx remotion browser ensure ile indirme yapma).
// Öncelik: REMOTION_BROWSER_EXECUTABLE ortam değişkeni → Playwright'ın Chromium'u.
// Tam Chromium (headless shell değil) kullanılır: sistem NSS sertifika deposunu okur, böylece kurumsal/ara
// sunucu sertifikalarıyla da Google Fonts indirilebilir. Bu yüzden "chrome-for-testing" modu.
const findBrowser = (): string | null => {
  const fromEnv = process.env.REMOTION_BROWSER_EXECUTABLE;
  if (fromEnv && fs.existsSync(fromEnv)) return fromEnv;

  const root = process.env.PLAYWRIGHT_BROWSERS_PATH ?? "/opt/pw-browsers";
  const candidates = [path.join(root, "chromium")];
  if (fs.existsSync(root)) {
    for (const dir of fs.readdirSync(root).sort().reverse()) {
      if (/^chromium-\d+$/.test(dir)) candidates.push(path.join(root, dir, "chrome-linux", "chrome"));
    }
  }
  return candidates.find((c) => fs.existsSync(c)) ?? null;
};

const browser = findBrowser();
if (browser) {
  Config.setBrowserExecutable(browser);
  Config.setChromeMode("chrome-for-testing");
}

Config.setVideoImageFormat("jpeg");
Config.setOverwriteOutput(true);
