import { useEffect, useState, FC } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Spinner, Dialog, Button, Text, TextField, Flex, Callout } from '@radix-ui/themes';
import { Info, TriangleAlert } from 'lucide-react';
import { GameList } from './GameList';
import { Page } from './Page';
import { PageTitle } from './PageTitle';
import { PageSelect } from './PageSelect';
import { getGameList, GameSummary, GameListData } from './GameListData';

export const GamesPage: FC = () => {
  const { userName, page } = useParams();
  const navigate = useNavigate();

  const [gamelist, setGameList] = useState<GameSummary | null>(null);
  const [loading, setLoading] = useState(true);

  
  const [pageMsg, setPageMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  

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
          <GameList userName={userName || ''} data={displayData} />

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

