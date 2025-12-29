import { ReactNode } from 'react';
import { Dialog, Button, Flex, Callout, Spinner } from '@radix-ui/themes';
import { AlertTriangle } from 'lucide-react';

interface Props {
  trigger?: ReactNode;
  open?: boolean;
  title: string;
  description: ReactNode;
  confirmLabel: string;
  confirmColor?: 'red' | 'green' | 'blue';
  loading?: boolean; 
  error?: string | null; 
  onConfirm: () => void;
  onCancel?: () => void;
}

export const ConfirmDialog = ({
  trigger,
  open,
  title,
  description,
  confirmLabel,
  confirmColor = 'blue',
  loading = false,
  error = null,
  onConfirm,
  onCancel,
}: Props) => {
  return (
    <Dialog.Root
      open={open}
      
      onOpenChange={(isOpen) => !isOpen && !loading && onCancel && onCancel()}
    >
      {trigger && <Dialog.Trigger onClick={(e) => e.stopPropagation()}>{trigger}</Dialog.Trigger>}

      <Dialog.Content  maxWidth="450px" onClick={(e) => e.stopPropagation()}>
        <Dialog.Title>{title}</Dialog.Title>
        <Dialog.Description size="2" mb="4">
          {description}
        </Dialog.Description>

        {/* ERROR MESSAGE (Callout) */}
        {error && (
          <Callout.Root color="red" mb="4">
            <Callout.Icon>
              <AlertTriangle size={16} />
            </Callout.Icon>
            <Callout.Text>{error}</Callout.Text>
          </Callout.Root>
        )}

        <Flex gap="3" mt="4" justify="end">
          <Dialog.Close>
            {/* Disable Cancel while loading so they can't interrupt the process */}
            <Button
              variant="soft"
              color="gray"
              onClick={onCancel}
              disabled={loading}
              style={{ cursor: loading ? 'not-allowed' : 'pointer' }}
            >
              Cancel
            </Button>
          </Dialog.Close>

          <Button
            variant="solid"
            color={confirmColor}
            onClick={onConfirm}
            disabled={loading}
            style={{ cursor: loading ? 'not-allowed' : 'pointer', minWidth: '80px' }}
          >
            {loading ? <Spinner /> : confirmLabel}
          </Button>
        </Flex>
      </Dialog.Content>
    </Dialog.Root>
  );
};
