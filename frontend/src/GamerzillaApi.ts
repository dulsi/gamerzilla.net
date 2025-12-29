import { http, HttpResponse } from './http';

export interface ManagedGame {
  id: number;
  gameName: string;
  shortName: string;
  ownerName: string;
}

export interface PagedResult<T> {
  data: T[];
  total: number;
}

export interface TransferRequest {
  gameId: number;
  newOwnerUsername: string;
}

export const getOwnedGames = async (page = 1, pageSize = 50): Promise<PagedResult<ManagedGame>> => {
  try {
    const result = await http<undefined, PagedResult<ManagedGame>>({
      path: `/Gamerzilla/game/list/owned?page=${page}&pageSize=${pageSize}`,
      method: 'get',
    });

    if (result.parsedBody) {
      return result.parsedBody;
    } else {
      return { data: [], total: 0 };
    }
  } catch (ex) {
    console.error(ex);
    return { data: [], total: 0 };
  }
};

export const transferGame = async (gameId: number, newOwnerUsername: string): Promise<string> => {
  try {
    const body: TransferRequest = { gameId, newOwnerUsername };

    const result = await http<TransferRequest, string>({
      path: '/Gamerzilla/game/transfer',
      method: 'post',
      body: body,
    });

    return result.parsedBody || 'Success';
  } catch (ex: any) {
    const errorText = ex.parsedBody || ex.message;
    throw new Error(errorText || 'Transfer failed');
  }
};

export const verifyGameTransfer = async (token: string): Promise<string> => {
  try {
    const result = await http<undefined, string>({
      path: `/user/verify?token=${token}`,
      method: 'post',
    });

    return result.parsedBody || 'Success';
  } catch (ex: any) {
    const errorText = ex.parsedBody || ex.message;
    throw new Error(errorText || 'Transfer failed');
  }
};
