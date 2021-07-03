import { http } from "./http";

export interface GameListData {
  shortname: string;
  name: string;
  earned: number;
  total: number;
}

export interface GameData {
  shortname: string;
  name: string;
  version: string;
  trophy: GameTrophyData[];
}

export interface GameTrophyData {
  trophy_name: string;
  trophy_desc: string;
  achieved: string;
  progress: string;
  max_progress: string;
}

const gameEmpty: GameData = {
  shortname: '',
  name: '',
  version: "",
  trophy: []
};

export const getGameList = async (userName: string):
  Promise<GameListData[]> => {
    try {
      const result = await http<
        undefined,
        GameListData[]
      >({
        path: '/games?username=' + userName,
      });
      if (result.parsedBody) {
        return result.parsedBody;
      } else {
        return [];
      }
    } catch (ex) {
      console.error(ex);
      return [];
    }
  };

export const getGame = async (userName: string, shortName: string):
  Promise<GameData> => {
    try {
      const result = await http<
        undefined,
        GameData
      >({
        path: '/game?game=' + shortName + '&username=' + userName,
      });
      if (result.parsedBody) {
        return result.parsedBody;
      } else {
        return gameEmpty;
      }
    } catch (ex) {
      console.error(ex);
      return gameEmpty;
    }
  };
