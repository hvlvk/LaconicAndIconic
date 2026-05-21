(() => {
    const notificationHost = document.getElementById("notificationHost");

    if (!notificationHost || !window.signalR) {
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", notification => {
        console.info("SignalR notification received.", notification);

        const item = document.createElement("div");
        item.className = "notification-toast";

        const title = document.createElement("strong");
        title.textContent = notification.title ?? "Notification";

        const message = document.createElement("span");
        message.textContent = notification.message ?? "";

        item.append(title, message);
        notificationHost.appendChild(item);

        window.setTimeout(() => {
            item.classList.add("notification-toast-hidden");
            item.addEventListener("transitionend", () => item.remove(), { once: true });
        }, 7000);
    });

    const startConnection = () => {
        connection.start()
            .then(() => console.info("SignalR notification connection started."))
            .catch(error => {
                console.error("SignalR notification connection failed.", error);
                window.setTimeout(startConnection, 5000);
            });
    };

    startConnection();
})();
