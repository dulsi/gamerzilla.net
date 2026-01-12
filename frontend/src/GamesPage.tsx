import { useEffect, useState, FC } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Spinner, Dialog, Button, Text, TextField, Flex, Callout } from '@radix-ui/themes';
import { Info, TriangleAlert } from 'lucide-react';
import { GameList } from './GameList';
import { Page } from './Page';
import { PageTitle } from './PageTitle';
import { PageSelect } from './PageSelect';
import { getGameList, GameSummary, GameListData } from './GameListData';
import { transferGame } from './GamerzillaApi';
import { defaultServerOptions, getServerOptions } from './User';
import { TransferDialog } from './TransferDialog';

export const GamesPage: FC = () => {
  const { userName, page } = useParams();
  const navigate = useNavigate();

  const [gamelist, setGameList] = useState<GameSummary | null>(null);
  const [loading, setLoading] = useState(true);

  
  const [transferTarget, setTransferTarget] = useState<GameListData | null>(null);

  
  const [pageMsg, setPageMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [transferError, setTransferError] = useState<string | null>(null);
  const [isTransferring, setIsTransferring] = useState(false);
  const [serverOpts, setServerOpts] = useState(defaultServerOptions);
  


    useEffect(() => {
      getServerOptions().then(setServerOpts);
    }, []);

  const fetchGames = async () => {
    if (userName) {
      setLoading(true);
      const pageNum = parseInt(page ?? '0');
      const list = await getGameList(userName, pageNum);
      setGameList(list);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchGames();
  }, [userName, page]);

  const handlePageChange = (p: number) => navigate(`/games/${userName}/${p}`);

const handleTransfer = async (username: string) => {
  if (!transferTarget) return;

  setIsTransferring(true);
  setTransferError(null);

  try {
    
    const responseMessage = await transferGame(transferTarget.id, username);

    
    const isPendingEmail = serverOpts.emailEnabled;

    setPageMsg({
      type: isPendingEmail ? 'error' : 'success', 
      text: responseMessage, 
    });

    
    setTransferTarget(null);

    
    
    if (!isPendingEmail) {
      fetchGames();
    }
  } catch (e: any) {
    const msg = e.parsedBody || e.message || 'Transfer failed. Please check the username.';
    setTransferError(msg);
  } finally {
    setIsTransferring(false);
  }
};

  const openTransferDialog = (game: GameListData) => {
    setPageMsg(null);
    setTransferError(null);
    setTransferTarget(game);
  };

  const displayData = gamelist?.games || [];
  const showPagination = gamelist && gamelist.totalPages > 1;

  
  //const mockedData = displayData.length > 0 ? new Array(20).fill(displayData).flat() : [];

  return (
    <Page>
      <Box mb="5">
        <PageTitle>Games List - {userName}</PageTitle>
      </Box>

      {/* MAIN PAGE SUCCESS MESSAGE */}
      {pageMsg && (
        <Callout.Root
          color={pageMsg.type === 'success' ? 'green' : 'red'}
          variant="soft"
          style={{ marginBottom: 20 }}
        >
          <Callout.Icon>
            <Info size={16} />
          </Callout.Icon>
          <Callout.Text>{pageMsg.text}</Callout.Text>
        </Callout.Root>
      )}

      {loading ? (
        <Flex justify="center" align="center" style={{ height: '50vh' }}>
          <Spinner size="3" />
        </Flex>
      ) : (
        <Box>
          {showPagination && (
            <Box mb="4">
              <PageSelect
                currentPage={gamelist?.currentPage || 0}
                totalPages={gamelist?.totalPages || 0}
                onPageChange={handlePageChange}
              />
            </Box>
          )}

          {/* Pass the userName string directly to ensure routing works */}
          <GameList userName={userName || ''} data={displayData} onTransfer={openTransferDialog} />

          <TransferDialog
  target={transferTarget}
  isTransferring={isTransferring}
  error={transferError}
  onClose={() => setTransferTarget(null)}
  onConfirm={handleTransfer}
/>

          {showPagination && (
            <Box mt="6">
              <PageSelect
                currentPage={gamelist?.currentPage || 0}
                totalPages={gamelist?.totalPages || 0}
                onPageChange={handlePageChange}
              />
            </Box>
          )}
        </Box>
      )}
      </Page>
  );
};

