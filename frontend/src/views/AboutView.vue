<template>
  <div class="min-h-screen w-full bg-slate-50 text-slate-900 font-sans overflow-x-hidden selection:bg-purple-200">
    
    <!-- Background Elements -->
    <div class="fixed top-0 left-0 w-full h-full overflow-hidden -z-10 pointer-events-none">
      <div class="absolute top-[-10%] right-[-5%] w-[30rem] h-[30rem] bg-purple-200/40 rounded-full blur-3xl mix-blend-multiply animate-blob"></div>
      <div class="absolute bottom-[-10%] left-[-10%] w-[35rem] h-[35rem] bg-indigo-200/40 rounded-full blur-3xl mix-blend-multiply animate-blob animation-delay-2000"></div>
    </div>

    <div class="max-w-4xl mx-auto px-6 py-24 lg:py-32">
      
      <!-- Header -->
      <div class="text-center mb-20">
        <h1 v-motion :initial="{ opacity: 0, y: 20 }" :enter="{ opacity: 1, y: 0 }" class="text-4xl md:text-6xl font-extrabold tracking-tight text-slate-900 mb-6">
          Building the <span class="text-transparent bg-clip-text bg-gradient-to-r from-purple-600 to-indigo-600">Future of Connection</span>
        </h1>
        <p v-motion :initial="{ opacity: 0, y: 20 }" :enter="{ opacity: 1, y: 0 }" :delay="100" class="text-lg text-slate-600 max-w-2xl mx-auto leading-relaxed">
          TrojoNetworks isn't just a platform; it's a philosophy. We believe in a web where identity travels with you, and communities are built on your terms.
        </p>
      </div>

      <!-- Content Sections -->
      <div class="space-y-24">
        
        <!-- Section 1: The Problem -->
        <div v-motion-slide-visible-once-bottom class="flex flex-col md:flex-row items-center gap-12">
          <div class="flex-1">
            <div class="w-16 h-16 rounded-2xl bg-red-100 text-red-500 flex items-center justify-center mb-6">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
              </svg>
            </div>
            <h2 class="text-3xl font-bold mb-4">The Fragmentation Problem</h2>
            <p class="text-slate-600 leading-relaxed text-lg">
              Today's web is siloed. Your reputation, your data, and your identity are locked inside walled gardens. Moving between communities often means starting over from scratch. We realized that for the web to truly evolve, the barriers between networks need to dissolve.
            </p>
          </div>
          <div class="flex-1 bg-white p-8 rounded-3xl shadow-xl shadow-slate-200/50 border border-slate-100 rotate-3 hover:rotate-0 transition duration-500">
            <div class="space-y-4 opacity-50 grayscale hover:grayscale-0 transition duration-500">
              <div class="h-4 bg-slate-200 rounded w-3/4"></div>
              <div class="h-4 bg-slate-200 rounded w-full"></div>
              <div class="h-4 bg-slate-200 rounded w-5/6"></div>
              <div class="h-4 bg-slate-200 rounded w-4/5"></div>
            </div>
          </div>
        </div>

        <!-- Section 2: Our Solution -->
        <div v-motion-slide-visible-once-bottom class="flex flex-col md:flex-row-reverse items-center gap-12">
          <div class="flex-1">
            <div class="w-16 h-16 rounded-2xl bg-green-100 text-green-500 flex items-center justify-center mb-6">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h2 class="text-3xl font-bold mb-4">A Unified Ecosystem</h2>
            <p class="text-slate-600 leading-relaxed text-lg">
              We built TrojoNetworks to be the bridge. By allowing account reuse and providing granular permission controls, we empower creators to build distinct communities that still feel part of a larger whole. It's about sovereignty without isolation.
            </p>
          </div>
          <div class="flex-1 bg-gradient-to-br from-purple-600 to-indigo-600 p-8 rounded-3xl shadow-xl shadow-purple-500/20 -rotate-3 hover:rotate-0 transition duration-500 text-white">
            <div class="font-mono text-sm opacity-80 mb-2">Identity_Token: Verified</div>
            <div class="text-2xl font-bold mb-4">User: @Traveler</div>
            <div class="flex gap-2">
              <span class="px-3 py-1 rounded-full bg-white/20 text-xs">Network A</span>
              <span class="px-3 py-1 rounded-full bg-white/20 text-xs">Network B</span>
              <span class="px-3 py-1 rounded-full bg-white/20 text-xs">Global</span>
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
  0% { transform: translate(0px, 0px) scale(1); }
  33% { transform: translate(30px, -50px) scale(1.1); }
  66% { transform: translate(-20px, 20px) scale(0.9); }
  100% { transform: translate(0px, 0px) scale(1); }
}
</style>
