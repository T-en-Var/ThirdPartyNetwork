<template>
  <div class="container">
    <div class="logo-container">
      <img src="@/images/logo_transparent.png" alt="Company Logo">
      <div class="star star-top-right"/>
      <div class="star star-bottom-left"/>
    </div>
    <div class="content">
      <header>
        <span class="title">Connect your Web</span>
      </header>

      <main>
        <div>
          <span>Thirdpartynetwork is a way to connect the web.</span>
        </div>

        <div>
          <span>Create and manage public or private networks with customizable permissions.</span>
        </div>

        <div>
          <span>Allow users to reuse accounts used in other networks.</span>
        </div>
      </main>

      <div class="buttons">
        <button class="btn primary">Get Started</button>
        <button class="btn secondary">Learn More</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

</script>

<style scoped>
.container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  min-width: 100vw;
  padding: 2rem;
  font-family: 'Helvetica Neue', Arial, sans-serif;
  z-index: 0;
}

.logo-container {
  position: fixed;
  width: 100%;
  max-width: 600px;
  margin-bottom: 20rem;
  z-index: 1;
}

.logo-circle {
  position: relative;
  width: 100%;
  height: 0;
  padding-bottom: 100%;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(255, 240, 170, 0.9) 20%, rgba(190, 149, 230, 0.85) 80%);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  border: 1px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 10px 30px rgba(149, 104, 210, 0.2);
}

.logo-text {
  position: absolute;
  font-size: 4vw;
  font-weight: 500;
  color: #121212;
  text-align: center;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 90%;
}

.star {
  position: absolute;
  width: 40px;
  height: 40px;
  background-color: #000;
  clip-path: polygon(50% 0%, 61% 35%, 98% 35%, 68% 57%, 79% 91%,
      50% 70%, 21% 91%, 32% 57%, 2% 35%, 39% 35%);
}

.star-top-right {
  top: 10%;
  right: 5%;
}

.star-bottom-left {
  bottom: 10%;
  left: 5%;
}

.content {
  width: 100%;
  max-width: 1200px;
  text-align: center;
  border-top: 1px solid var(--primary-color);
  border-radius: 1.5rem;
  box-shadow: 3px 5px 7px rgba(0, 0, 0, 0.2);
  background-color: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(30px);
  -webkit-backdrop-filter: blur(10px);
  filter: drop-shadow(0 0 3px rgba(255, 255, 255, 1));
  z-index: 10;
}

.title {
  font-size: 2rem;
  font-weight: 700;
  padding: .5rem 0 1rem;
  display: inline-block;
  background: linear-gradient(90deg, var(--primary-accent-color), var(--primary-color));
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tagline {
  font-size: 1.5rem;
  font-weight: 500;
  margin-bottom: 1.5rem;
  color: #333;
}

.description {
  font-size: 1.2rem;
  color: #555;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
  margin-top: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  font-weight: 500;
  border-radius: 2rem;
  cursor: pointer;
  border: none;
  transition: all 0.3s ease;
}

.primary {
  background-color: #9966cc;
  color: white;
}

.primary:hover {
  background-color: #8a4fc8;
  box-shadow: 0 4px 12px rgba(149, 104, 210, 0.3);
}

.secondary {
  background-color: transparent;
  color: #9966cc;
  border: 1px solid #9966cc;
}

.secondary:hover {
  background-color: rgba(153, 102, 204, 0.1);
}

@media (max-width: 768px) {
  .logo-text {
    font-size: 2rem;
  }

  .tagline {
    font-size: 1.2rem;
  }

  .description {
    font-size: 1rem;
  }
}
</style>
