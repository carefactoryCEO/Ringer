<template>
  <div class="about">
    <section id="room-list" :class="{ invisible: isEntered }">
      <h3>대화방 목록</h3>
      <a v-for="room in rooms" :key="room.Id" @click="enterRoom(room.Id)">{{
        room.Name
      }}</a>
    </section>

    <ul id="chat-feed" class="chat">
      <li
        v-for="item in items"
        :key="item.id"
        class="message-cell"
        :class="item.me ? 'mine' : 'yours'"
      >
        <div class="message">
          {{ item.body }}
        </div>
      </li>
    </ul>

    <div class="input-bar" :class="{ invisible: !isEntered }">
      <input
        type="text"
        id="message-input"
        v-model="messageToSend"
        placeholder="메세지"
      />
      <button @click="sendMeessage">보내</button>
    </div>
  </div>
</template>

<script>
import { v4 as uuid } from "uuid";
import axios from "axios";
import Vue from "vue";

export default {
  data: function() {
    return {
      isEntered: false,
      rooms: [],
      items: [],
      messageToSend: ""
    };
  },
  created() {
    this.$connection.on(
      "ReceiveMessage",
      (senderName, body, messageId, userId, createdAt) => {
        const message = {
          senderName,
          body,
          messageId,
          userId,
          createdAt
        };
        this.addItem(`${message.senderName}: ${message.body}`);
      }
    );
  },
  mounted() {
    // get room list
    this.getRoomList();

    this.$connection.onreconnecting(e => console.log(e));
    /* eslint-disable */
    this.$connection.onreconnected(e => {
      let test = "";
      console.log(e);
      this.enterRoom(this.$currentRoomId);
    });
    /** eslint-enable */
    this.$connection.onclose(e => {
      console.log(e);
    });

    // enter to send
    document.querySelector("#message-input").addEventListener("keypress", e => {
      if (e.keyCode == 13) this.sendMeessage();
    });

    document.querySelector("#message-input").addEventListener("focus", e => {
      e.target.placeholder = "";
    });

    document.querySelector("#message-input").addEventListener("focusout", e => {
      e.target.placeholder = "메시지";
    });
  },
  methods: {
    enterRoom(roomId) {
      this.$connection.invoke("AddToGroup", roomId, "똥개");
      console.log(`Entered to ${roomId}`);
      Vue.prototype.$currentRoomId = roomId;
      this.isEntered = true;
    },
    async getRoomList() {
      await axios
        .get("/auth/list")
        .then(r => (this.rooms = r.data))
        .catch(err => console.log(err));

      console.log(this.rooms);
    },
    sendMeessage() {
      this.$connection.invoke(
        "SendMessageToRoomAsyc",
        this.messageToSend,
        this.$currentRoomId
      );

      this.addItem(this.messageToSend, true);

      this.messageToSend = "";

      document.querySelector("#message-input").focus();
    },
    addItem(body, me = false) {
      const id = uuid();
      this.items.push({ id: id, body, me });

      this.scrollToBottom();
    },
    scrollToBottom() {
      setTimeout(() => {
        const element = document.getElementById("chat-feed");
        element.scrollTop = element.scrollHeight - element.clientHeight;
      }, 1);
    }
  }
};
</script>

<style scoped>
/**
https://codepen.io/swards/pen/gxQmbj
*/
a {
  text-decoration: underline;
}
a:hover {
  cursor: pointer;
}
.input-bar {
  display: flex;
  justify-content: space-around;
}
.invisible {
  display: none;
}
.about {
  font-family: helvetica;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.chat {
  overflow: scroll;
  height: 400px;
  width: 300px;
  border: solid 1px #eee;
  padding: 10px;
}

.message-cell {
  display: flex;
  flex-direction: column;
}

.message {
  border-radius: 20px;
  padding: 8px 15px;
  margin-top: 5px;
  margin-bottom: 5px;
  display: inline-block;
  text-align: start;
}

.yours {
  align-items: flex-start;
}
.yours .message {
  margin-right: 25%;
  background-color: #eee;
  position: relative;
}

.yours .message.last:before {
  content: "";
  position: absolute;
  z-index: 0;
  bottom: 0;
  left: -7px;
  height: 20px;
  width: 20px;
  background: #eee;
  border-bottom-right-radius: 15px;
}
.yours .message.last:after {
  content: "";
  position: absolute;
  z-index: 1;
  bottom: 0;
  left: -10px;
  width: 10px;
  height: 20px;
  background: white;
  border-bottom-right-radius: 10px;
}

.mine {
  align-items: flex-end;
}

.mine .message {
  color: white;
  margin-left: 25%;
  background: linear-gradient(to bottom, #00d0ea 0%, #0085d1 100%);
  background-attachment: fixed;
  position: relative;
}
</style>
