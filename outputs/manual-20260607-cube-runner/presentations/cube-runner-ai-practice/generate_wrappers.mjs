import fs from "node:fs/promises";
import path from "node:path";
import { slideCount } from "./slides/common.mjs";

const root = "C:/TowerJumpingLevel2/outputs/manual-20260607-cube-runner/presentations/cube-runner-ai-practice";
const slidesDir = path.join(root, "slides");
const count = slideCount;

await fs.mkdir(slidesDir, { recursive: true });

for (let i = 1; i <= count; i += 1) {
  const n = String(i).padStart(2, "0");
  const content = `import { makeSlide } from "./common.mjs";

export async function slide${n}(presentation, ctx) {
  return makeSlide(presentation, ctx, ${i});
}
`;
  await fs.writeFile(path.join(slidesDir, `slide-${n}.mjs`), content, "utf8");
}

console.log(`Generated ${count} slide wrappers in ${slidesDir}`);
