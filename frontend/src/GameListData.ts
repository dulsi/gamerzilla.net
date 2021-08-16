import { http } from "./http";

export interface GameSummary {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  games: GameListData[];
}

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

const gameSummaryEmpty: GameSummary = {
  currentPage: 0,
  pageSize: 20,
  totalPages: 0,
  games: []
};

export const getGameList = async (userName: string, page: number):
  Promise<GameSummary> => {
    try {
      const result = await http<
        undefined,
        GameSummary
      >({
        path: '/gamerzilla/games?username=' + userName + '&currentpage=' + page,
      });
      if (result.parsedBody) {
        return result.parsedBody;
      } else {
        return gameSummaryEmpty;
      }
    } catch (ex) {
      console.error(ex);
      return gameSummaryEmpty;
    }
  };

export const getGame = async (userName: string, shortName: string):
  Promise<GameData> => {
    try {
      const result = await http<
        undefined,
        GameData
      >({
        path: '/gamerzilla/game?game=' + shortName + '&username=' + userName,
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
