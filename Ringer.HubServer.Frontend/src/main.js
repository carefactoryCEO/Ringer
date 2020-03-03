import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import signalR from "./plugins/signalR";

Vue.config.productionTip = false;
Vue.use(signalR); // calls signalR.install(Vue)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount("#app");
