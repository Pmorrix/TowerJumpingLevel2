import fs from "node:fs/promises";
import path from "node:path";
import { slideCount } from "./slides/common.mjs";

const root = "C:/TowerJumpingLevel2/outputs/manual-20260608-cube-runner-redesign/presentations/cube-runner-ai-practice";
const slidesDir = path.join(root, "slides");

await fs.mkdir(slidesDir, { recursive: true });

for (let i = 1; i <= slideCount; i += 1) {
  const n = String(i).padStart(2, "0");
  await fs.writeFile(
    path.join(slidesDir, `slide-${n}.mjs`),
    `import { makeSlide } from "./common.mjs";

export async function slide${n}(presentation, ctx) {
  return makeSlide(presentation, ctx, ${i});
}
`,
    "utf8",
  );
}

console.log(`Generated ${slideCount} slide wrappers`);
