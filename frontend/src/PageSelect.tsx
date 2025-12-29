import { FC } from 'react';
import { Flex, Button, Text, Box } from '@radix-ui/themes';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface Props {
  currentPage: number;
  totalPages: number;
  
  onPageChange: (newPage: number) => void;
  isLoading?: boolean;
}

export const PageSelect: FC<Props> = ({
  currentPage,
  totalPages,
  onPageChange,
  isLoading = false,
}) => {
  const getPageNumbers = () => {
    const pages = [];
    let start = Math.max(0, currentPage - 2);
    let end = Math.min(totalPages - 1, start + 4);
    if (end - start < 4) start = Math.max(0, end - 4);

    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  };

  if (totalPages <= 1) return null;

  return (
    <Flex
      align="center"
      justify="center" 
      my="6"
      style={{ width: '100%' }}
      gap="5" 
    >
      {/* 1. Previous Button */}
      <Button
        disabled={currentPage <= 0 || isLoading}
        variant="ghost"
        color="gray"
        onClick={() => onPageChange(currentPage - 1)}
        style={{ padding: '0 12px', cursor: currentPage <= 0 ? 'default' : 'pointer' }}
      >
        <ChevronLeft size={18} />
     
          Previous
 
      </Button>

      {/* 2. Numeric Container */}
      <Flex gap="1" align="center">
        {/* First Page Shortcut */}
        {currentPage > 2 && (
          <>
            <Button variant="ghost" color="gray" onClick={() => onPageChange(0)}>
              1
            </Button>
            {currentPage > 3 && (
              <Text color="gray" mx="1" style={{ alignSelf: 'center', opacity: 0.5 }}>
                ...
              </Text>
            )}
          </>
        )}

        {/* Dynamic Numbers */}
        {getPageNumbers().map((pageNum) => (
          <Button
            key={pageNum}
            variant={currentPage === pageNum ? 'outline' : 'ghost'}
            color="gray"
            highContrast={currentPage === pageNum}
            onClick={() => onPageChange(pageNum)}
            disabled={isLoading}
            style={{
              minWidth: '40px',
              height: '40px',
              cursor: 'pointer',
              
              border: currentPage === pageNum ? '1px solid var(--gray-7)' : '1px solid transparent',
            }}
          >
            {pageNum + 1}
          </Button>
        ))}

        {/* Last Page Shortcut */}
        {currentPage < totalPages - 3 && (
          <>
            {currentPage < totalPages - 4 && (
              <Text color="gray" mx="1" style={{ alignSelf: 'center', opacity: 0.5 }}>
                ...
              </Text>
            )}
            <Button variant="ghost" color="gray" onClick={() => onPageChange(totalPages - 1)}>
              {totalPages}
            </Button>
          </>
        )}
      </Flex>

      {/* 3. Next Button */}
      <Button
        disabled={currentPage + 1 >= totalPages || isLoading}
        variant="ghost"
        color="gray"
        onClick={() => onPageChange(currentPage + 1)}
        style={{ padding: '0 12px', cursor: currentPage + 1 >= totalPages ? 'default' : 'pointer' }}
      >
   
          Next
   
        <ChevronRight size={18} />
      </Button>
    </Flex>
  );
};