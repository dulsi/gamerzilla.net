import { FC, useState, useEffect } from 'react';
import { Dialog, Button, Text, TextField, Flex, Callout, Spinner } from '@radix-ui/themes';
import { TriangleAlert } from 'lucide-react';
import { GameListData } from './GameListData';

interface Props {
  target: GameListData | null;
  onClose: () => void;
  onConfirm: (username: string) => void;
  isTransferring: boolean;
  error: string | null;
}

export const TransferDialog: FC<Props> = ({
  target,
  onClose,
  onConfirm,
  isTransferring,
  error,
}) => {
  const [localUsername, setLocalUsername] = useState('');

  useEffect(() => {
    if (target) setLocalUsername('');
  }, [target]);

  return (
    <Dialog.Root open={!!target} onOpenChange={(o) => !o && !isTransferring && onClose()}>
      <Dialog.Content style={{ maxWidth: 450 }}>
        <Dialog.Title>Transfer Ownership</Dialog.Title>
        <Dialog.Description size="2" mb="4">
          Transferring{' '}
          <Text weight="bold" color="red">
            {target?.name}
          </Text>
          .
        </Dialog.Description>

        <Flex direction="column" gap="3">
          <label>
            <Text as="div" size="2" mb="1" weight="bold">
              Recipient Username
            </Text>
            <TextField.Root
              size="3"
              value={localUsername}
              autoFocus
              onChange={(e) => setLocalUsername(e.target.value)}
              placeholder="Enter exact username"
            />
          </label>

          {error && (
            <Callout.Root color="red" size="1">
              <Callout.Icon>
                <TriangleAlert size={14} />
              </Callout.Icon>
              <Callout.Text>{error}</Callout.Text>
            </Callout.Root>
          )}
        </Flex>

        <Flex gap="3" mt="4" justify="end">
          <Dialog.Close>
            <Button variant="soft" color="gray" disabled={isTransferring}>
              Cancel
            </Button>
          </Dialog.Close>
          <Button
            color="red"
            onClick={() => onConfirm(localUsername)}
            disabled={!localUsername || isTransferring}
          >
            {isTransferring ? <Spinner /> : 'Confirm Transfer'}
          </Button>
        </Flex>
      </Dialog.Content>
    </Dialog.Root>
  );
};
