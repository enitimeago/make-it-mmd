#!/bin/bash

# Define source and target directories
source_dir="../../../../../third_party/Linguini/Linguini.Bundle"
target_dir="."

# Ensure the target directory exists
if [ ! -d "$target_dir" ]; then
  echo "Target directory does not exist: $target_dir"
  exit 1
fi

# Iterate over all *.cs files in the source directory
find "$source_dir" -type f -name "*.cs" | while read -r source_file; do
  # Compute the relative path and target file path
  relative_path="${source_file#$source_dir/}"
  target_file="$target_dir/$relative_path"

  # Ensure the target directory structure exists
  mkdir -p "$(dirname "$target_file")"

  # Remove the existing file if it exists
  if [ -e "$target_file" ]; then
    rm "$target_file"
  fi

  # Create a symlink
  ln -s "$source_file" "$target_file"
  echo "Created symlink: $target_file -> $source_file"
done
