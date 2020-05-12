import {
  HubConnectionBuilder,
  LogLevel,
  HubConnectionState,
} from "@microsoft/signalr";
import axios from "axios";
import { v4 as uuid } from "uuid";

export default {
  token: "",
  userId: -1,

  async getToken() {
    await axios
      .post("/auth/staff-login", {
        email: "test@test.com",
        password: "string",
        devicetype: 2, // DeviceType.Web
        deviceId: uuid(),
        /**
         * iOS      = 0,
         * Android  = 1,
         * Web      = 2,
         * Desktop  = 3,
         * Console  = 4
         */
      })
      .then((response) => {
        this.token = response.data.token;
        this.userId = response.data.userId;
      });

    return this.token;
  },

  async install(Vue) {
    await this.getToken();
    Vue.prototype.$token = this.token;
    Vue.prototype.$userId = this.userId;

    console.log(`token: ${this.token}`);

    const connection = new HubConnectionBuilder()
      .withUrl("/hubs/chat", {
        accessTokenFactory: () => this.getToken(),
      })
      .withAutomaticReconnect([0, 0, 1000, 1000, 1000, 2000, 2000])
      .configureLogging(LogLevel.Information)
      .build();

    Vue.prototype.$connection = connection;

    connection.on("Entered", (name) => console.log(`${name} entered to room`));
    connection.on("Left", (name) => console.log(`${name} left to room`));

    connection.on(
      "ReceiveMessage",
      (senderName, body, messageId, userId, createdAt) => {
        const receivedMessage = {
          senderName,
          body,
          messageId,
          userId,
          createdAt,
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
  },
};
