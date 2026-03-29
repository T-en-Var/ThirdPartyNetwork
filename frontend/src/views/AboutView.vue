<template>
  <div
    class="min-h-screen w-full overflow-x-hidden bg-slate-50 font-sans text-slate-900 selection:bg-purple-200"
  >
    <!-- Background Elements -->
    <div class="pointer-events-none fixed left-0 top-0 -z-10 h-full w-full overflow-hidden">
      <div
        class="animate-blob absolute right-[-5%] top-[-10%] h-[30rem] w-[30rem] rounded-full bg-purple-200/40 mix-blend-multiply blur-3xl"
      ></div>
      <div
        class="animate-blob animation-delay-2000 absolute bottom-[-10%] left-[-10%] h-[35rem] w-[35rem] rounded-full bg-indigo-200/40 mix-blend-multiply blur-3xl"
      ></div>
    </div>

    <div class="mx-auto max-w-4xl px-6 py-24 lg:py-32">
      <!-- Header -->
      <div class="mb-20 text-center">
        <h1
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          class="mb-6 text-4xl font-extrabold tracking-tight text-slate-900 md:text-6xl"
        >
          Building the
          <span class="bg-gradient-to-r from-purple-600 to-indigo-600 bg-clip-text text-transparent"
            >Future of Connection</span
          >
        </h1>
        <p
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          :delay="100"
          class="mx-auto max-w-2xl text-lg leading-relaxed text-slate-600"
        >
          TrojoNetworks isn't just a platform; it's a philosophy. We believe in a web where identity
          travels with you, and communities are built on your terms.
        </p>
      </div>

      <!-- Content Sections -->
      <div class="space-y-24">
        <!-- Section 1: The Problem -->
        <div
          v-motion-slide-visible-once-bottom
          class="flex flex-col items-center gap-12 md:flex-row"
        >
          <div class="flex-1">
            <div
              class="mb-6 flex h-16 w-16 items-center justify-center rounded-2xl bg-red-100 text-red-500"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-8 w-8"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"
                />
              </svg>
            </div>
            <h2 class="mb-4 text-3xl font-bold">The Fragmentation Problem</h2>
            <p class="text-lg leading-relaxed text-slate-600">
              Today's web is siloed. Your reputation, your data, and your identity are locked inside
              walled gardens. Moving between communities often means starting over from scratch. We
              realized that for the web to truly evolve, the barriers between networks need to
              dissolve.
            </p>
          </div>
          <div
            class="flex-1 rotate-3 rounded-3xl border border-slate-100 bg-white p-8 shadow-xl shadow-slate-200/50 transition duration-500 hover:rotate-0"
          >
            <div class="space-y-4 opacity-50 grayscale transition duration-500 hover:grayscale-0">
              <div class="h-4 w-3/4 rounded bg-slate-200"></div>
              <div class="h-4 w-full rounded bg-slate-200"></div>
              <div class="h-4 w-5/6 rounded bg-slate-200"></div>
              <div class="h-4 w-4/5 rounded bg-slate-200"></div>
            </div>
          </div>
        </div>

        <!-- Section 2: Our Solution -->
        <div
          v-motion-slide-visible-once-bottom
          class="flex flex-col items-center gap-12 md:flex-row-reverse"
        >
          <div class="flex-1">
            <div
              class="mb-6 flex h-16 w-16 items-center justify-center rounded-2xl bg-green-100 text-green-500"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-8 w-8"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
            </div>
            <h2 class="mb-4 text-3xl font-bold">A Unified Ecosystem</h2>
            <p class="text-lg leading-relaxed text-slate-600">
              We built TrojoNetworks to be the bridge. By allowing account reuse and providing
              granular permission controls, we empower creators to build distinct communities that
              still feel part of a larger whole. It's about sovereignty without isolation.
            </p>
          </div>
          <div
            class="flex-1 -rotate-3 rounded-3xl bg-gradient-to-br from-purple-600 to-indigo-600 p-8 text-white shadow-xl shadow-purple-500/20 transition duration-500 hover:rotate-0"
          >
            <div class="mb-2 font-mono text-sm opacity-80">Identity_Token: Verified</div>
            <div class="mb-4 text-2xl font-bold">User: @Traveler</div>
            <div class="flex gap-2">
              <span class="rounded-full bg-white/20 px-3 py-1 text-xs">Network A</span>
              <span class="rounded-full bg-white/20 px-3 py-1 text-xs">Network B</span>
              <span class="rounded-full bg-white/20 px-3 py-1 text-xs">Global</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-blob {
  animation: blob 7s infinite;
}
.animation-delay-2000 {
  animation-delay: 2s;
}
@keyframes blob {
  0% {
    transform: translate(0px, 0px) scale(1);
  }
  33% {
    transform: translate(30px, -50px) scale(1.1);
  }
  66% {
    transform: translate(-20px, 20px) scale(0.9);
  }
  100% {
    transform: translate(0px, 0px) scale(1);
  }
}
</style>
