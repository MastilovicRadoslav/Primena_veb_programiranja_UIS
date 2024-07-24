const { override, addWebpackResolve } = require('customize-cra');

module.exports = override(
  addWebpackResolve({
    fallback: {
      os: require.resolve('os-browserify/browser'),
      crypto: require.resolve('crypto-browserify'),
      buffer: require.resolve('buffer/'),
      vm: require.resolve('vm-browserify'),
      stream: require.resolve('stream-browserify') // Add this line
    }
  })
);
