"use strict";

var sendButton = document.getElementById("sendButton");
var userInput = document.getElementById("userInput");
var messageInput = document.getElementById("messageInput");

var url = "https://localhost:5001/hubs/chat"; // local
//var url = "https://ringerchat.azurewebsites.net/hubs/chat"; // azure:jhylmb@naver.com

console.error(url);

var group = "Xamarin";
var user = userInput.value = "Web" + getInt();

//Disable send button until connection is established
sendButton.disabled = true;

// init
var connection = new signalR.HubConnectionBuilder().withUrl(url).build();

connection.on("ReceiveMessage", function (user, message) {

    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + ": " + msg;

    var li = document.createElement("li");
    li.textContent = encodedMsg;

    document.getElementById("messagesList").prepend(li);
});


// connect
connection.start().then(function () {

    sendButton.disabled = false;
    userInput.disabled = true;

    console.log("connected");
    joinchannel();

}).catch(function (err) {

    return console.log(err.toString());
});


// join
function joinchannel() {

    connection.invoke("AddToGroup", group, user)
        .then(function () {
            return console.log("joined to Xamarin");
        })
        .catch(function (err) {
            return console.log(err.toString());
        });

}

sendButton.addEventListener("click", function (event) {

    sendMessage();

    event.preventDefault();
});

messageInput.addEventListener("keypress", function (e) {

    if (e.keyCode == 13) {

        sendMessage();

        e.preventDefault();
    }

});

function sendMessage() {

    var message = messageInput.value;

    connection
        .invoke("SendMessageGroup", group, user, message)
        .catch(function (err) {
            return console.error(err.toString());
        });

    messageInput.value = '';
}

function getInt() {
    return Math.floor(Math.random() * (100 - 1 + 1)) + 1;
}