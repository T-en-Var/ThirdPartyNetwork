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
          Building the
          <span class="page-hero__title-highlight">Future of Connection</span>
        </h1>
        <p
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :enter="{ opacity: 1, y: 0 }"
          :delay="100"
          class="page-hero__lead"
        >
          TrojoNetworks isn't just a platform; it's a philosophy. We believe in a web where identity
          travels with you, and communities are built on your terms.
        </p>
      </div>
    </div>

    <div class="page-hero__container">
      <!-- Content Sections -->
      <div class="about-page__sections">
        <!-- Section 1: The Problem -->
        <div
          v-motion-slide-visible-once-bottom
          class="about-page__section"
        >
          <div class="about-page__section-copy">
            <div class="about-page__section-icon about-page__section-icon--danger">
              <svg
                xmlns="http://www.w3.org/2000/svg"
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
            <h2 class="about-page__section-title">The Fragmentation Problem</h2>
            <p class="about-page__section-text">
              Today's web is siloed. Your reputation, your data, and your identity are locked inside
              walled gardens. Moving between communities often means starting over from scratch. We
              realized that for the web to truly evolve, the barriers between networks need to
              dissolve.
            </p>
          </div>
          <div class="about-page__section-visual">
            <div class="about-page__section-visual-lines">
              <div class="about-page__section-visual-line about-page__section-visual-line--w-3-4"></div>
              <div class="about-page__section-visual-line about-page__section-visual-line--w-full"></div>
              <div class="about-page__section-visual-line about-page__section-visual-line--w-5-6"></div>
              <div class="about-page__section-visual-line about-page__section-visual-line--w-4-5"></div>
            </div>
          </div>
        </div>

        <!-- Section 2: Our Solution -->
        <div
          v-motion-slide-visible-once-bottom
          class="about-page__section about-page__section--reverse"
        >
          <div class="about-page__section-copy">
            <div class="about-page__section-icon about-page__section-icon--success">
              <svg
                xmlns="http://www.w3.org/2000/svg"
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
            <h2 class="about-page__section-title">A Unified Ecosystem</h2>
            <p class="about-page__section-text">
              We built TrojoNetworks to be the bridge. By allowing account reuse and providing
              granular permission controls, we empower creators to build distinct communities that
              still feel part of a larger whole. It's about sovereignty without isolation.
            </p>
          </div>
          <div class="about-page__section-card">
            <div class="about-page__section-card-meta">Identity_Token: Verified</div>
            <div class="about-page__section-card-user">User: @Traveler</div>
            <div class="about-page__section-card-tags">
              <span class="badge badge--on-dark">Network A</span>
              <span class="badge badge--on-dark">Network B</span>
              <span class="badge badge--on-dark">Global</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
