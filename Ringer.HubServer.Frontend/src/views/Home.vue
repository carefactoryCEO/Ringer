<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <button @click="showConnectionId">보여줘</button>
    <HelloWorld msg="Welcome to Your Vue.js App" />
  </div>
</template>

<script>
// @ is an alias to /src
import HelloWorld from "@/components/HelloWorld.vue";

export default {
  name: "home",
  components: {
    HelloWorld
  },
  methods: {
    showConnectionId() {
      console.log(`connection id: ${this.$connectionId}
room id: ${this.$currentRoomId}`);
    }
  }
};
</script>
