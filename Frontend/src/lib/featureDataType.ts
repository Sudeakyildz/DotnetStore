/** Backend FeatureDataType enum */
export const FeatureDataType = {
  None: 0,
  String: 1,
  Int: 2,
  Decimal: 3,
  Date: 4,
  Bool: 5,
} as const;

export function featureDataTypeLabel(code: number): string {
  switch (code) {
    case FeatureDataType.None:
      return '—';
    case FeatureDataType.String:
      return 'string';
    case FeatureDataType.Int:
      return 'int';
    case FeatureDataType.Decimal:
      return 'decimal';
    case FeatureDataType.Date:
      return 'date';
    case FeatureDataType.Bool:
      return 'bool';
    default:
      return String(code);
  }
}
