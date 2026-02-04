<template>
  <div class="w-full bg-slate-50 text-slate-900 font-sans overflow-x-hidden selection:bg-purple-200">
    
    <!-- Background Elements -->
    <div class="fixed top-0 left-0 w-full h-full overflow-hidden -z-10 pointer-events-none">
      <div class="absolute top-[-10%] right-[-5%] w-[30rem] h-[30rem] bg-purple-200/40 rounded-full blur-3xl mix-blend-multiply animate-blob"></div>
      <div class="absolute bottom-[-10%] left-[-10%] w-[35rem] h-[35rem] bg-indigo-200/40 rounded-full blur-3xl mix-blend-multiply animate-blob animation-delay-2000"></div>
    </div>

    <div class="max-w-4xl mx-auto px-6 py-24 lg:py-32">
      
      <!-- Header -->
      <div class="text-center mb-20">
        <h1 v-motion :initial="{ opacity: 0, y: 20 }" :enter="{ opacity: 1, y: 0 }" class="text-4xl md:text-6xl font-extrabold tracking-tight text-slate-900 mb-6">
          Contact
        </h1>
        <p v-motion :initial="{ opacity: 0, y: 20 }" :enter="{ opacity: 1, y: 0 }" :delay="100" class="text-lg text-slate-600 max-w-2xl mx-auto leading-relaxed">
          AI halucinated this page link, so I created a place holder. You can't really contact me. (yet) 😜
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
  0% { transform: translate(0px, 0px) scale(1); }
  33% { transform: translate(30px, -50px) scale(1.1); }
  66% { transform: translate(-20px, 20px) scale(0.9); }
  100% { transform: translate(0px, 0px) scale(1); }
}
</style>
