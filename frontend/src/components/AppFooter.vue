<template>
  <footer class="w-full bg-white border-t border-slate-200 py-8 mt-auto">
    <div class="max-w-7xl mx-auto px-6 flex flex-col md:flex-row justify-between items-center gap-4">
      <div class="flex flex-col md:flex-row items-center gap-2 md:gap-8">
        <span class="font-bold text-slate-700 tracking-tight">TrojoNetworks</span>
        <span class="text-slate-500 text-sm hidden md:inline">|</span>
        <span class="text-slate-500 text-sm">&copy; {{ new Date().getFullYear() }} All rights reserved.</span>
      </div>
      <div class="flex gap-6 text-sm font-medium text-slate-600">
        <router-link to="/contact" class="hover:text-purple-600 transition-colors duration-200">Contact</router-link>
        <router-link to="/tos" class="hover:text-purple-600 transition-colors duration-200">Terms of Service</router-link>
      </div>
    </div>
  </footer>
</template>