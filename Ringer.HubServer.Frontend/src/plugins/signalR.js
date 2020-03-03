import {
  HubConnectionBuilder,
  LogLevel,
  HubConnectionState
} from "@microsoft/signalr";
import axios from "axios";

export default {
  token: "",

  async getToken() {
    await axios
      .post("/auth/staff-login", {
        email: "test@test.com",
        password: "string"
      })
      .then(response => {
        this.token = response.data.token;
      });

    return this.token;
  },

  install(Vue) {
    const connection = new HubConnectionBuilder()
      .withUrl("/hubs/chat", {
        accessTokenFactory: () => this.getToken()
      })
      .configureLogging(LogLevel.Information)
      .build();

    Vue.prototype.$connection = connection;

    connection.on("Entered", name => console.log(`${name} entered to room`));
    connection.on("Left", name => console.log(`${name} left to room`));

    connection.on(
      "ReceiveMessage",
      (senderName, body, messageId, userId, createdAt) => {
        const receivedMessage = {
          senderName,
          body,
          messageId,
          userId,
          createdAt
        };
        console.log(receivedMessage);
      }
    );

    connection.start().then(() => {
      if (connection.state == HubConnectionState.Connected) {
        console.log(
          `\t\tconnected. connection id == ${connection.connectionId}`
        );

        Vue.prototype.$connectionId = connection.connectionId;
      }
    });
  }
};
