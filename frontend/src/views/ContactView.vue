<template>
  <div class="page">
    <div class="page-hero">
      <!-- Background Elements -->
      <div class="page-hero__bg">
        <div class="page-hero__blob"></div>
        <div class="page-hero__blob page-hero__blob--delay-2000"></div>
      </div>

      <div class="page-hero__container">
        <h1
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          class="page-hero__title"
        >
          Contact
        </h1>
        <p
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          :delay="100"
          class="page-hero__lead"
        >
          AI halucinated this page link, so I created a place holder. You can't really contact me.
          (yet) 😜
        </p>
      </div>
    </div>
  </div>
</template>
