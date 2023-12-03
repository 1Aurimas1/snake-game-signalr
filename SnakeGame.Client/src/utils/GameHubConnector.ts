import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { GameMode } from "../shared/constants/GameMode";
import { Grid } from "../shared/interfaces/Grid";
import { Direction } from "../shared/constants/Directions";

class GameHubConnector {
  private connection: HubConnection;

  constructor(accessToken: string) {
    this.connection = new HubConnectionBuilder()
      .withUrl("/gamehub", {
        accessTokenFactory: () => accessToken,
      })
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.onclose((error) => {
      if (error) {
        console.error(
          "[onclose] SignalR connection closed with an error:",
          error,
        );
      } else {
        console.log(
          "[onclose] SignalR connection closed by the server or due to other reasons.",
        );
      }
    });
  }

  async waitForCondition(
    conditionFn: (s: HubConnectionState) => boolean,
    state: HubConnectionState,
  ) {
    while (conditionFn(state)) {
      await new Promise((resolve) => setTimeout(resolve, 1000));
    }
  }

  async waitForState(state: HubConnectionState) {
    await this.waitForCondition((s) => this.connection.state !== s, state);
  }

  async waitUntilStateChange(state: HubConnectionState) {
    await this.waitForCondition((s) => this.connection.state === s, state);
  }

  public async initGrid(): Promise<Grid> {
    await this.waitForState(HubConnectionState.Connected);
    const tuple = await this.connection.invoke("InitGrid");
    return {
      rows: tuple.item1,
      columns: tuple.item2,
    };
  }

  public async joinGame(
    playerName: string,
    selectedMode: GameMode,
  ): Promise<string> {
    await this.waitForState(HubConnectionState.Connected);
    return await this.connection.invoke("JoinGame", playerName, selectedMode);
  }

  public async sendUserInput(
    roomId: string,
    playerName: string,
    newDirection: Direction,
  ) {
    await this.waitForState(HubConnectionState.Connected);
    await this.connection
      .send("SendUserInput", roomId, playerName, newDirection)
      .catch((e) => console.error("[SendUserInput] error: ", e));
  }

  public async startConnection() {
    await this.connection.start().catch((err) => console.error(err));
  }

  public async stopConnection() {
    await this.waitUntilStateChange(HubConnectionState.Connecting);
    await this.connection.stop();
  }

  public async on(methodName: string, newMethod: (...args: any[]) => any) {
    this.connection.on(methodName, newMethod);
  }

  public off(methodName: string) {
    this.connection.off(methodName);
  }
}

export default GameHubConnector;
