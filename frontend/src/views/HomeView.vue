<template>
  <div class="page">
    <!-- Hero Section -->
    <section class="page-section page-hero">
      <div
        class="page-hero__title"
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        <span>Trojo's Networks</span>
      </div>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        Welcome to my network! <br />
        Do you want one too?
      </p>
    </section>

    <section class="page-section">
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        New here?
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        Try these applications:
      </p>
      <ul
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        <li>
          <a
            href="https://athomeserver.trojonetworks.com"
            class="link"
            target="_blank"
            >AtHomeServer</a
          >
        </li>
        <li>
          <a
            href="https://vexiolobase.trojonetworks.com"
            class="link"
            target="_blank"
            >VexioloBase</a
          >
        </li>
      </ul>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        They use TN :D
      </p>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        AtHomeServer is currently almost fully integrated with TN, and VexioloBase is fully
        standalone.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        But I plan to make these fully integrated in the future!
      </p>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        Do you want to build your own app ontop of TN? I'd be grateful!
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="400">
        Log in and head to the
        <RouterLink
          to="networks/create"
          class="link"
          >network creation page</RouterLink
        >
        and play around. Everything is totally free (for now, and for a long time).
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="400">
        There are some limitations in order to not roast my server, but I can loosen these
        limitations if needed.
      </p>
    </section>
  </div>
</template>

<script setup lang="ts"></script>
