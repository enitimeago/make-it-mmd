import os
import subprocess

def main():
    target_dir = os.path.join("third_party", "Linguini", "PluralRules.Generator")

    if not os.path.isdir(target_dir):
        print(f"Error: Directory '{target_dir}' does not exist.")
        return

    try:
        print(f"Building project in: {target_dir}")
        result = subprocess.run(["dotnet", "build", target_dir], check=True)
        print("Build completed successfully.")
    except subprocess.CalledProcessError as e:
        print(f"Build failed with error code: {e.returncode}")

if __name__ == "__main__":
    main()
