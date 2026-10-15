<template>
  <div class="page">
    <section class="page-section page-hero">
      <div
        class="page-hero__title"
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        <span>Some more information</span>
      </div>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        It's really cool. <br />
        I promise!
      </p>
    </section>

    <section class="page-section">
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        This a hobby project I have started for no reason other than the joy of building a project
        by hand and learning alongside it.
      </p>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        The ultimate goal is to have mutliple other hobby projects use this one as a main platform
        to use it as a:
      </p>
      <ul
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        <li>third party login service</li>
        <li>content storage</li>
      </ul>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        This allows me to keep all the data in a centralized, secure, server.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        But also allow users to reuse accounts throughout my applications.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        This also means that application made by other people than me could use this system to their
        advantage.
      </p>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        But for users of my applications, I want the experience to be as smooth and seemless as
        possible.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        For this I'm planning on adding other ways of logging in, like OAuth and other providers to
        make it even more accessible.
      </p>
    </section>

    <section class="page-section">
      <h2
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
      >
        How it works
      </h2>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="100"
      >
        All data is stored according to the
        <RouterLink
          to="/tos"
          class="link"
          >ToS</RouterLink
        >.
      </p>
      <br />
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        A network has users.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="200"
      >
        You can give these users roles which permissions you give the role.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        You can request access to data of users through "Accesses". Users must comply first, your
        users will always know what data you have of them.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="300"
      >
        You can make Accesses optional, if you kinda want to have them, but don't want to block the
        user from using your network if they don't wish to share that information.
      </p>
      <br />
      <h3
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="400"
      >
        Content
      </h3>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="500"
      >
        If allowed by the network, a user of the said network can upload content to the said
        networks database.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="500"
      >
        This content is private by default, but can be shared with others. Inside and outside the
        said network.
      </p>
      <p
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :enter="{ opacity: 1, y: 0 }"
        :delay="600"
      >
        I've tried to make this as flexible as possible, so please let me know if you experience any
        issues, errors or bugs.
      </p>
    </section>
  </div>
</template>
