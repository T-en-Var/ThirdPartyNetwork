<template>
  <footer class="mt-auto w-full border-t border-slate-200 bg-white py-8">
    <div
      class="mx-auto flex max-w-7xl flex-col items-center justify-between gap-4 px-6 md:flex-row"
    >
      <div class="flex flex-col items-center gap-2 md:flex-row md:gap-8">
        <span class="font-bold tracking-tight text-slate-700">TrojoNetworks</span>
        <span class="hidden text-sm text-slate-500 md:inline">|</span>
        <span class="text-sm text-slate-500"
          >&copy; {{ new Date().getFullYear() }} All rights reserved.</span
        >
      </div>
      <div class="flex gap-6 text-sm font-medium text-slate-600">
        <router-link
          to="/contact"
          class="transition-colors duration-200 hover:text-purple-600"
          >Contact</router-link
        >
        <router-link
          to="/tos"
          class="transition-colors duration-200 hover:text-purple-600"
          >Terms of Service</router-link
        >
      </div>
    </div>
  </footer>
</template>
