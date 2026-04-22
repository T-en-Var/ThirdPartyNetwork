<template>
  <div
    class="w-full overflow-x-hidden bg-slate-50 font-sans text-slate-900 selection:bg-purple-200"
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
          Contact
        </h1>
        <p
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          :delay="100"
          class="mx-auto max-w-2xl text-lg leading-relaxed text-slate-600"
        >
          AI halucinated this page link, so I created a place holder. You can't really contact me.
          (yet) 😜
        </p>
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
