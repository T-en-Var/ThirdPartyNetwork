<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white shadow-md rounded-lg overflow-hidden text-center py-16">
      <div class="flex flex-col items-center justify-center">
        <div class="bg-red-50 rounded-full p-6 mb-6">
          <svg class="h-16 w-16 text-red-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <h1 class="text-6xl font-bold text-gray-800 mb-2">404</h1>
        <p class="text-xl text-gray-600 mb-8">Page not found</p>
        <router-link 
          to="/" 
          class="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Return Home
        </router-link>
      </div>
    </div>
  </div>
</template>
